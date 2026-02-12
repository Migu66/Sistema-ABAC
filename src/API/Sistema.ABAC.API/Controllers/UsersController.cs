using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para la gestión de usuarios del sistema ABAC.
/// Proporciona endpoints para operaciones CRUD de usuarios y gestión de sus atributos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos los endpoints requieren autenticación
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Constructor del controlador de usuarios.
    /// </summary>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // Los endpoints se implementarán en los siguientes pasos:
    // - GET /api/users (Paso 50)
    // - GET /api/users/{id} (Paso 51)
    // - PUT /api/users/{id} (Paso 52)
    // - DELETE /api/users/{id} (Paso 53)
    // - GET /api/users/{id}/attributes (Paso 54)
    // - POST /api/users/{id}/attributes (Paso 55)
    // - DELETE /api/users/{id}/attributes/{attributeId} (Paso 56)
}

