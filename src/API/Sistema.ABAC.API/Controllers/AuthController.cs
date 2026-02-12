using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.Services;

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

    // Los endpoints (Login, Profile, Refresh) se implementarán en los próximos pasos
}
