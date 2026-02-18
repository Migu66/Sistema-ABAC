using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Implementación del servicio de autenticación.
/// Gestiona el registro, login y generación de tokens JWT para usuarios.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly IAbacDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<User> userManager,
        IMapper mapper,
        IJwtService jwtService,
        IAbacDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _mapper = mapper;
        _jwtService = jwtService;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<TokenDto> RegisterAsync(RegisterDto registerDto)
    {
        // Verificar si el nombre de usuario ya existe
        var existingUser = await _userManager.FindByNameAsync(registerDto.UserName);
        if (existingUser != null)
        {
            throw new ValidationException("UserName", "El nombre de usuario ya está en uso.");
        }

        // Verificar si el correo ya existe
        var existingEmail = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingEmail != null)
        {
            throw new ValidationException("Email", "El correo electrónico ya está registrado.");
        }

        // Mapear RegisterDto a User
        var user = _mapper.Map<User>(registerDto);
        user.Id = Guid.NewGuid();

        // Crear el usuario con Identity
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            // Convertir errores de Identity a diccionario de errores de validación
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );

            throw new ValidationException(errors);
        }

        // Asignar rol por defecto (si existe configuración de roles)
        // await _userManager.AddToRoleAsync(user, "User");

        // Generar y devolver token
        return await GenerateTokenAsync(user.Id);
    }

    /// <inheritdoc />
    public async Task<TokenDto> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuario por nombre de usuario
        var user = await _userManager.FindByNameAsync(loginDto.UserName);

        if (user == null)
        {
            throw new ValidationException("UserName", "Credenciales inválidas.");
        }

        // Verificar si el usuario está eliminado (soft delete)
        if (user.IsDeleted)
        {
            throw new ValidationException("UserName", "Esta cuenta ha sido desactivada.");
        }

        // Verificar contraseña
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!isPasswordValid)
        {
            throw new ValidationException("Password", "Credenciales inválidas.");
        }

        // Verificar si el usuario debe confirmar email (opcional)
        // if (!user.EmailConfirmed)
        // {
        //     throw new ValidationException("Email", "Debe confirmar su correo electrónico antes de iniciar sesión.");
        // }

        // Actualizar fecha de último acceso (si se desea)
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generar y devolver token
        return await GenerateTokenAsync(user.Id);
    }

    /// <inheritdoc />
    public async Task<TokenDto> GenerateTokenAsync(Guid userId)
    {
        // Buscar usuario
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("User", userId);
        }

        // Obtener roles del usuario
        var roles = await _userManager.GetRolesAsync(user);

        // Obtener claims del usuario (atributos adicionales)
        var claims = await _userManager.GetClaimsAsync(user);

        // Mapear usuario a UserDto
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles.ToList();

        // Generar token JWT usando el servicio JWT
        var token = await _jwtService.GenerateTokenAsync(user, roles, claims);

        // Guardar refresh token en la base de datos
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token.RefreshToken!,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Los refresh tokens duran 7 días
            IsRevoked = false,
            CreatedByIp = GetClientIpAddress()
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return new TokenDto
        {
            AccessToken = token.AccessToken,
            TokenType = token.TokenType,
            ExpiresIn = token.ExpiresIn,
            ExpiresAt = token.ExpiresAt,
            RefreshToken = token.RefreshToken,
            User = userDto
        };
    }

    /// <inheritdoc />
    public async Task<TokenDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        // Validar que los tokens no estén vacíos
        if (string.IsNullOrEmpty(refreshTokenDto.AccessToken) || 
            string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
        {
            throw new ValidationException("Token", "Los tokens proporcionados no son válidos.");
        }

        // Buscar el refresh token en la base de datos
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);

        if (storedToken == null)
        {
            throw new ValidationException("RefreshToken", "El token de actualización no es válido.");
        }

        // Verificar que el token esté activo (no revocado y no expirado)
        if (!storedToken.IsActive)
        {
            throw new ValidationException("RefreshToken", 
                storedToken.IsRevoked 
                    ? "El token de actualización ha sido revocado." 
                    : "El token de actualización ha expirado.");
        }

        // Verificar que el usuario existe y está activo
        if (storedToken.User.IsDeleted)
        {
            throw new ValidationException("User", "La cuenta de usuario ha sido desactivada.");
        }

        // Validar que el token JWT corresponde al usuario del refresh token
        var tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwtToken = null;
        
        try
        {
            jwtToken = tokenHandler.ReadJwtToken(refreshTokenDto.AccessToken);
        }
        catch
        {
            throw new ValidationException("AccessToken", "El token de acceso no es válido.");
        }

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || 
            !Guid.TryParse(userIdClaim, out var userId) || 
            userId != storedToken.UserId)
        {
            throw new ValidationException("Token", "Los tokens no corresponden al mismo usuario.");
        }

        // Revocar el token anterior
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        // Generar nuevos tokens
        var newTokenDto = await GenerateTokenAsync(storedToken.UserId);

        // Guardar el token antiguo revocado con referencia al nuevo
        var newRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == newTokenDto.RefreshToken);
        
        if (newRefreshToken != null)
        {
            storedToken.ReplacedByTokenId = newRefreshToken.Id;
        }

        await _dbContext.SaveChangesAsync();

        return newTokenDto;
    }

    /// <summary>
    /// Obtiene la dirección IP del cliente desde el contexto HTTP.
    /// </summary>
    /// <returns>Dirección IP del cliente o null si no está disponible.</returns>
    private string? GetClientIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
