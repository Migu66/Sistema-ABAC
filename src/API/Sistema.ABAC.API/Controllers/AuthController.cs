using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.Services;
using System.Security.Claims;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para la gestión de autenticación y autorización de usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Constructor del controlador de autenticación.
    /// </summary>
    /// <param name="authService">Servicio de autenticación.</param>
    /// <param name="logger">Servicio de logging.</param>
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
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

    // El endpoint Refresh se implementará en el próximo paso
}
