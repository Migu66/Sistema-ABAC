using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.DTOs.Common;
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

    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
    /// <param name="searchTerm">Término de búsqueda para filtrar por nombre, email o username</param>
    /// <param name="department">Filtrar por departamento</param>
    /// <param name="isActive">Filtrar por estado activo (true) o inactivo (false)</param>
    /// <param name="sortBy">Campo para ordenar: UserName, Email, FullName, CreatedAt (por defecto UserName)</param>
    /// <param name="sortDescending">Orden descendente (por defecto false)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de usuarios</returns>
    /// <response code="200">Lista de usuarios obtenida exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<UserDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? department = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string sortBy = "UserName",
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo lista de usuarios - Página: {Page}, Tamaño: {PageSize}, Búsqueda: {SearchTerm}",
            page, pageSize, searchTerm);

        var result = await _userService.GetAllAsync(
            page, pageSize, searchTerm, department, isActive, sortBy, sortDescending, cancellationToken);

        return Ok(result);
    }

    // Los siguientes endpoints se implementarán en los próximos pasos:
    // - GET /api/users/{id} (Paso 51)
    // - PUT /api/users/{id} (Paso 52)
    // - DELETE /api/users/{id} (Paso 53)
    // - GET /api/users/{id}/attributes (Paso 54)
    // - POST /api/users/{id}/attributes (Paso 55)
    // - DELETE /api/users/{id}/attributes/{attributeId} (Paso 56)
}

