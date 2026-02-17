using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.API.Security;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para la gestión de autenticación y autorización de usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Constructor del controlador de autenticación.
    /// </summary>
    /// <param name="authService">Servicio de autenticación.</param>
    /// <param name="tokenBlacklistService">Servicio de blacklist de tokens.</param>
    /// <param name="logger">Servicio de logging.</param>
    public AuthController(
        IAuthService authService,
        ITokenBlacklistService tokenBlacklistService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _tokenBlacklistService = tokenBlacklistService;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="registerDto">Datos del usuario a registrar.</param>
    /// <returns>Token JWT del usuario registrado.</returns>
    /// <response code="201">Usuario registrado exitosamente.</response>
    /// <response code="400">Datos de registro inválidos o usuario ya existe.</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Registrar usuario",
        Description = "Ejemplo request: {\n  \"userName\": \"jdoe\",\n  \"email\": \"jdoe@abac.com\",\n  \"password\": \"P@ssw0rd!\",\n  \"confirmPassword\": \"P@ssw0rd!\",\n  \"fullName\": \"John Doe\",\n  \"department\": \"IT\"\n}\n\nEjemplo response 201: {\n  \"accessToken\": \"eyJ...\",\n  \"tokenType\": \"Bearer\",\n  \"expiresIn\": 3600,\n  \"expiresAt\": \"2026-02-17T11:00:00Z\",\n  \"refreshToken\": \"rt_...\",\n  \"user\": { \"id\": \"00000000-0000-0000-0000-000000000000\", \"userName\": \"jdoe\", \"email\": \"jdoe@abac.com\" }\n}")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenDto>> Register([FromBody] RegisterDto registerDto)
    {
        _logger.LogInformation("Iniciando registro de usuario: {UserName}", registerDto.UserName);

        var result = await _authService.RegisterAsync(registerDto);

        _logger.LogInformation("Usuario registrado exitosamente: {UserName}", registerDto.UserName);

        return CreatedAtAction(nameof(Register), new { id = result.User.Id }, result);
    }

    /// <summary>
    /// Autentica un usuario existente en el sistema.
    /// </summary>
    /// <param name="loginDto">Credenciales de inicio de sesión.</param>
    /// <returns>Token JWT del usuario autenticado.</returns>
    /// <response code="200">Autenticación exitosa.</response>
    /// <response code="401">Credenciales inválidas.</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Iniciar sesión",
        Description = "Ejemplo request: {\n  \"userName\": \"jdoe\",\n  \"password\": \"P@ssw0rd!\",\n  \"rememberMe\": true\n}\n\nEjemplo response 200: {\n  \"accessToken\": \"eyJ...\",\n  \"tokenType\": \"Bearer\",\n  \"expiresIn\": 3600,\n  \"expiresAt\": \"2026-02-17T11:00:00Z\",\n  \"refreshToken\": \"rt_...\",\n  \"user\": { \"id\": \"00000000-0000-0000-0000-000000000000\", \"userName\": \"jdoe\", \"email\": \"jdoe@abac.com\" }\n}")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("Intento de inicio de sesión para usuario: {UserName}", loginDto.UserName);

        var result = await _authService.LoginAsync(loginDto);

        _logger.LogInformation("Inicio de sesión exitoso para usuario: {UserName}", loginDto.UserName);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado actualmente.
    /// </summary>
    /// <returns>Información del usuario autenticado.</returns>
    /// <response code="200">Perfil obtenido exitosamente.</response>
    /// <response code="401">Usuario no autenticado.</response>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        // Obtener el ID del usuario desde los claims del token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("No se pudo obtener el ID del usuario desde el token");
            return Unauthorized();
        }

        _logger.LogInformation("Obteniendo perfil para usuario ID: {UserId}", userId);

        var token = await _authService.GenerateTokenAsync(userId);

        return Ok(token.User);
    }

    /// <summary>
    /// Renueva un token JWT expirado usando un refresh token válido.
    /// </summary>
    /// <param name="refreshTokenDto">Token de acceso expirado y refresh token.</param>
    /// <returns>Nuevo token JWT.</returns>
    /// <response code="200">Token renovado exitosamente.</response>
    /// <response code="400">Tokens inválidos o expirados.</response>
    /// <response code="401">Refresh token no autorizado.</response>
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "Renovar token",
        Description = "Ejemplo request: {\n  \"accessToken\": \"eyJ...\",\n  \"refreshToken\": \"rt_...\"\n}\n\nEjemplo response 200: {\n  \"accessToken\": \"eyJ...new\",\n  \"tokenType\": \"Bearer\",\n  \"expiresIn\": 3600,\n  \"expiresAt\": \"2026-02-17T12:00:00Z\",\n  \"refreshToken\": \"rt_new...\",\n  \"user\": { \"id\": \"00000000-0000-0000-0000-000000000000\", \"userName\": \"jdoe\", \"email\": \"jdoe@abac.com\" }\n}")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("Intento de renovación de token");

        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        _logger.LogInformation("Token renovado exitosamente");

        return Ok(result);
    }

    /// <summary>
    /// Revoca el token actual agregándolo a la blacklist hasta su expiración.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Sin contenido.</returns>
    /// <response code="204">Token revocado correctamente.</response>
    /// <response code="401">Usuario no autenticado.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var tokenId = User.FindFirstValue(JwtRegisteredClaimNames.Jti)
            ?? User.FindFirstValue("jti");

        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp)
            ?? User.FindFirstValue("exp");

        if (string.IsNullOrWhiteSpace(tokenId) || !long.TryParse(expClaim, out var expUnix))
        {
            return Unauthorized();
        }

        var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        await _tokenBlacklistService.BlacklistTokenAsync(tokenId, expiresAtUtc, cancellationToken);

        _logger.LogInformation("Token revocado exitosamente. Jti={Jti}", tokenId);
        return NoContent();
    }
}
