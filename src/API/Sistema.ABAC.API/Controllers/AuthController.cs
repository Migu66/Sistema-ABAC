using Microsoft.AspNetCore.Mvc;
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

    // Los endpoints (Register, Login, Profile, Refresh) se implementarán en los próximos pasos
}
