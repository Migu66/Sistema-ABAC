using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para la gestión de recursos del sistema ABAC.
/// Proporciona endpoints para operaciones CRUD de recursos y gestión de sus atributos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos los endpoints requieren autenticación
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;
    private readonly ILogger<ResourcesController> _logger;

    /// <summary>
    /// Constructor del controlador de recursos.
    /// </summary>
    public ResourcesController(IResourceService resourceService, ILogger<ResourcesController> logger)
    {
        _resourceService = resourceService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Obtiene una lista paginada de recursos con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
    /// <param name="searchTerm">Término de búsqueda para filtrar por nombre, tipo o descripción</param>
    /// <param name="type">Filtrar por tipo de recurso (documento, endpoint, carpeta, etc.)</param>
    /// <param name="sortBy">Campo para ordenar: Name, Type, CreatedAt (por defecto Name)</param>
    /// <param name="sortDescending">Orden descendente (por defecto false)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de recursos</returns>
    /// <response code="200">Lista de recursos obtenida exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ResourceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<ResourceDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? type = null,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo recursos: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}, Type={Type}",
            page, pageSize, searchTerm, type);

        var result = await _resourceService.GetAllAsync(
            page, 
            pageSize, 
            searchTerm, 
            type, 
            sortBy, 
            sortDescending, 
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene un recurso específico por su ID.
    /// </summary>
    /// <param name="id">ID del recurso</param>
    /// <param name="includeAttributes">Incluir atributos del recurso (por defecto false)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos del recurso</returns>
    /// <response code="200">Recurso encontrado</response>
    /// <response code="404">Recurso no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResourceDto>> GetById(
        Guid id,
        [FromQuery] bool includeAttributes = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo recurso con ID: {ResourceId}, incluir atributos: {IncludeAttributes}", 
            id, includeAttributes);

        var resource = await _resourceService.GetByIdAsync(id, includeAttributes, cancellationToken);

        if (resource == null)
        {
            _logger.LogWarning("Recurso no encontrado: {ResourceId}", id);
            return NotFound(new { message = $"Recurso con ID {id} no encontrado" });
        }

        return Ok(resource);
    }

    /// <summary>
    /// Crea un nuevo recurso en el sistema.
    /// </summary>
    /// <param name="createDto">Datos del recurso a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Recurso creado</returns>
    /// <response code="201">Recurso creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResourceDto>> Create(
        [FromBody] CreateResourceDto createDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando nuevo recurso: {ResourceName} (Tipo: {ResourceType})", 
            createDto.Name, createDto.Type);

        var resource = await _resourceService.CreateAsync(createDto, cancellationToken);

        _logger.LogInformation("Recurso creado exitosamente: {ResourceId}", resource.Id);

        return CreatedAtAction(
            nameof(GetById), 
            new { id = resource.Id }, 
            resource);
    }

    /// <summary>
    /// Actualiza un recurso existente.
    /// </summary>
    /// <param name="id">ID del recurso a actualizar</param>
    /// <param name="updateDto">Datos actualizados del recurso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Recurso actualizado</returns>
    /// <response code="200">Recurso actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Recurso no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResourceDto>> Update(
        Guid id,
        [FromBody] UpdateResourceDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando recurso: {ResourceId}", id);

        var resource = await _resourceService.UpdateAsync(id, updateDto, cancellationToken);

        _logger.LogInformation("Recurso actualizado exitosamente: {ResourceId}", id);

        return Ok(resource);
    }

    /// <summary>
    /// Elimina un recurso del sistema (soft delete).
    /// </summary>
    /// <param name="id">ID del recurso a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Recurso eliminado exitosamente</response>
    /// <response code="404">Recurso no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando recurso: {ResourceId}", id);

        await _resourceService.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Recurso eliminado exitosamente: {ResourceId}", id);

        return NoContent();
    }

    #endregion

    #region Resource Attributes Management

    /// <summary>
    /// Obtiene todos los atributos asignados a un recurso específico.
    /// </summary>
    /// <param name="id">ID del recurso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de atributos del recurso</returns>
    /// <response code="200">Lista de atributos obtenida exitosamente</response>
    /// <response code="404">Recurso no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}/attributes")]
    [ProducesResponseType(typeof(List<ResourceAttributeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ResourceAttributeDto>>> GetAttributes(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo atributos del recurso: {ResourceId}", id);

        var attributes = await _resourceService.GetAttributesAsync(id, cancellationToken);

        _logger.LogInformation("Se obtuvieron {Count} atributos del recurso {ResourceId}", 
            attributes.Count, id);

        return Ok(attributes);
    }

    /// <summary>
    /// Asigna un atributo a un recurso con un valor específico.
    /// </summary>
    /// <param name="id">ID del recurso</param>
    /// <param name="assignDto">Datos del atributo a asignar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Atributo asignado</returns>
    /// <response code="201">Atributo asignado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos o atributo ya asignado</response>
    /// <response code="404">Recurso o atributo no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPost("{id:guid}/attributes")]
    [ProducesResponseType(typeof(ResourceAttributeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResourceAttributeDto>> AssignAttribute(
        Guid id,
        [FromBody] AssignResourceAttributeDto assignDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Asignando atributo {AttributeId} al recurso {ResourceId}", 
            assignDto.AttributeId, id);

        var resourceAttribute = await _resourceService.AssignAttributeAsync(id, assignDto, cancellationToken);

        _logger.LogInformation("Atributo asignado exitosamente al recurso {ResourceId}", id);

        return CreatedAtAction(
            nameof(GetAttributes),
            new { id },
            resourceAttribute);
    }

    /// <summary>
    /// Actualiza el valor de un atributo asignado a un recurso.
    /// </summary>
    /// <param name="id">ID del recurso</param>
    /// <param name="attributeId">ID del atributo</param>
    /// <param name="updateDto">Nuevo valor del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Atributo actualizado</returns>
    /// <response code="200">Atributo actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Recurso o atributo no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPut("{id:guid}/attributes/{attributeId:guid}")]
    [ProducesResponseType(typeof(ResourceAttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResourceAttributeDto>> UpdateAttribute(
        Guid id,
        Guid attributeId,
        [FromBody] UpdateResourceAttributeValueDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando atributo {AttributeId} del recurso {ResourceId}", 
            attributeId, id);

        var resourceAttribute = await _resourceService.UpdateAttributeAsync(
            id, 
            attributeId, 
            updateDto.Value, 
            cancellationToken);

        _logger.LogInformation("Atributo actualizado exitosamente");

        return Ok(resourceAttribute);
    }

    /// <summary>
    /// Remueve un atributo de un recurso.
    /// </summary>
    /// <param name="id">ID del recurso</param>
    /// <param name="attributeId">ID del atributo a remover</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Atributo removido exitosamente</response>
    /// <response code="404">Recurso o atributo no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpDelete("{id:guid}/attributes/{attributeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveAttribute(
        Guid id,
        Guid attributeId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removiendo atributo {AttributeId} del recurso {ResourceId}", 
            attributeId, id);

        await _resourceService.RemoveAttributeAsync(id, attributeId, cancellationToken);

        _logger.LogInformation("Atributo removido exitosamente del recurso {ResourceId}", id);

        return NoContent();
    }

    #endregion
}

/// <summary>
/// DTO para actualizar el valor de un atributo de recurso.
/// </summary>
public class UpdateResourceAttributeValueDto
{
    /// <summary>
    /// Nuevo valor del atributo.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
