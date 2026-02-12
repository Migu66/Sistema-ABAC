using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
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

    /// <summary>
    /// Obtiene un usuario específico por su ID.
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="includeAttributes">Incluir atributos del usuario (por defecto false)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del usuario</returns>
    /// <response code="200">Usuario encontrado</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetById(
        Guid id,
        [FromQuery] bool includeAttributes = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo usuario con ID: {UserId}", id);

        var user = await _userService.GetByIdAsync(id, includeAttributes, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Usuario con ID {UserId} no encontrado", id);
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Actualiza la información de un usuario existente.
    /// </summary>
    /// <param name="id">ID del usuario a actualizar</param>
    /// <param name="updateDto">Datos a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario actualizado</returns>
    /// <response code="200">Usuario actualizado exitosamente</response>
    /// <response code="400">Datos de actualización inválidos</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Update(
        Guid id,
        [FromBody] UpdateUserDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando usuario con ID: {UserId}", id);

        try
        {
            var user = await _userService.UpdateAsync(id, updateDto, cancellationToken);
            _logger.LogInformation("Usuario {UserId} actualizado exitosamente", id);
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Usuario no encontrado al intentar actualizar: {UserId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar usuario: {UserId}", id);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
    }

    /// <summary>
    /// Elimina un usuario del sistema (soft delete).
    /// El usuario se marca como eliminado pero no se borra físicamente de la base de datos.
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="204">Usuario eliminado exitosamente</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando usuario con ID: {UserId}", id);

        try
        {
            await _userService.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Usuario {UserId} eliminado exitosamente", id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Usuario no encontrado al intentar eliminar: {UserId}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    #region Gestión de Atributos

    /// <summary>
    /// Obtiene todos los atributos asignados a un usuario.
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="includeExpired">Incluir atributos expirados (por defecto false)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de atributos del usuario</returns>
    /// <response code="200">Lista de atributos obtenida exitosamente</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}/attributes")]
    [ProducesResponseType(typeof(IEnumerable<UserAttributeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserAttributeDto>>> GetUserAttributes(
        Guid id,
        [FromQuery] bool includeExpired = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo atributos del usuario {UserId}", id);

        try
        {
            var attributes = await _userService.GetUserAttributesAsync(id, includeExpired, cancellationToken);
            return Ok(attributes);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Usuario no encontrado al obtener atributos: {UserId}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    #endregion

    // Los siguientes endpoints se implementarán en los próximos pasos:
    // - POST /api/users/{id}/attributes (Paso 55)
    // - DELETE /api/users/{id}/attributes/{attributeId} (Paso 56)
}

