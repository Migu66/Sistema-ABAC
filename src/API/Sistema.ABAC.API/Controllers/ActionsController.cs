using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para la gestión de acciones del sistema ABAC.
/// Proporciona endpoints para operaciones CRUD de acciones que pueden realizarse sobre recursos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos los endpoints requieren autenticación
public class ActionsController : ControllerBase
{
    private readonly IActionService _actionService;
    private readonly ILogger<ActionsController> _logger;

    /// <summary>
    /// Constructor del controlador de acciones.
    /// </summary>
    public ActionsController(IActionService actionService, ILogger<ActionsController> logger)
    {
        _actionService = actionService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Obtiene una lista paginada de acciones con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
    /// <param name="searchTerm">Término de búsqueda para filtrar por nombre, código o descripción</param>
    /// <param name="sortBy">Campo para ordenar: Name, Code, CreatedAt (por defecto Name)</param>
    /// <param name="sortDescending">Orden descendente (por defecto false)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de acciones</returns>
    /// <response code="200">Lista de acciones obtenida exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ActionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<ActionDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo acciones: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}",
            page, pageSize, searchTerm);

        var result = await _actionService.GetAllAsync(
            page, 
            pageSize, 
            searchTerm, 
            sortBy, 
            sortDescending, 
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene una acción específica por su ID.
    /// </summary>
    /// <param name="id">ID de la acción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos de la acción</returns>
    /// <response code="200">Acción encontrada</response>
    /// <response code="404">Acción no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ActionDto>> GetById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo acción con ID: {ActionId}", id);

        var action = await _actionService.GetByIdAsync(id, cancellationToken);

        if (action == null)
        {
            _logger.LogWarning("Acción no encontrada: {ActionId}", id);
            return NotFound(new { message = $"Acción con ID {id} no encontrada" });
        }

        return Ok(action);
    }

    /// <summary>
    /// Obtiene una acción específica por su código único.
    /// </summary>
    /// <param name="code">Código de la acción (ej: "read", "write", "delete")</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos de la acción</returns>
    /// <response code="200">Acción encontrada</response>
    /// <response code="404">Acción no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// Este endpoint es útil para buscar acciones por su código único en lugar de su GUID.
    /// Por ejemplo: GET /api/actions/code/read
    /// </remarks>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ActionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ActionDto>> GetByCode(
        string code, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo acción con código: {Code}", code);

        var action = await _actionService.GetByCodeAsync(code, cancellationToken);

        if (action == null)
        {
            _logger.LogWarning("Acción no encontrada: {Code}", code);
            return NotFound(new { message = $"Acción con código '{code}' no encontrada" });
        }

        return Ok(action);
    }

    /// <summary>
    /// Crea una nueva acción en el sistema.
    /// </summary>
    /// <param name="createDto">Datos de la acción a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Acción creada</returns>
    /// <response code="201">Acción creada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos o código duplicado</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// El código de la acción debe ser único y seguir el formato snake_case (ej: "read", "delete", "approve_document").
    /// Una vez creado, el código no puede ser modificado.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ActionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ActionDto>> Create(
        [FromBody] CreateActionDto createDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando nueva acción: {ActionName} ({ActionCode})", 
            createDto.Name, createDto.Code);

        var action = await _actionService.CreateAsync(createDto, cancellationToken);

        _logger.LogInformation("Acción creada exitosamente: {ActionId}", action.Id);

        return CreatedAtAction(
            nameof(GetById), 
            new { id = action.Id }, 
            action);
    }

    /// <summary>
    /// Actualiza una acción existente.
    /// </summary>
    /// <param name="id">ID de la acción a actualizar</param>
    /// <param name="updateDto">Datos actualizados de la acción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Acción actualizada</returns>
    /// <response code="200">Acción actualizada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Acción no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// El código de la acción NO puede ser modificado después de creada.
    /// Solo se pueden actualizar el nombre y la descripción.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ActionDto>> Update(
        Guid id,
        [FromBody] UpdateActionDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando acción: {ActionId}", id);

        var action = await _actionService.UpdateAsync(id, updateDto, cancellationToken);

        _logger.LogInformation("Acción actualizada exitosamente: {ActionId}", id);

        return Ok(action);
    }

    /// <summary>
    /// Elimina una acción del sistema (soft delete).
    /// </summary>
    /// <param name="id">ID de la acción a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Acción eliminada exitosamente</response>
    /// <response code="400">La acción está en uso y no puede ser eliminada</response>
    /// <response code="404">Acción no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// La acción no puede ser eliminada si está asociada a políticas activas.
    /// El sistema realiza un soft delete, por lo que los registros históricos se mantienen.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando acción: {ActionId}", id);

        await _actionService.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Acción eliminada exitosamente: {ActionId}", id);

        return NoContent();
    }

    #endregion

    #region Validation Endpoints

    /// <summary>
    /// Verifica si existe una acción con el código especificado.
    /// </summary>
    /// <param name="code">Código a verificar</param>
    /// <param name="excludeId">ID de acción a excluir de la verificación (opcional, útil para updates)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la verificación</returns>
    /// <response code="200">Verificación completada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// Este endpoint es útil para validaciones en tiempo real en formularios del frontend.
    /// Por ejemplo: GET /api/actions/exists/code?code=read&amp;excludeId=...
    /// </remarks>
    [HttpGet("exists/code")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<bool>> ExistsByCode(
        [FromQuery] string code,
        [FromQuery] Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Verificando existencia de código: {Code}, excluir: {ExcludeId}", code, excludeId);

        var exists = await _actionService.ExistsByCodeAsync(code, excludeId, cancellationToken);

        return Ok(new { exists, code });
    }

    #endregion
}
