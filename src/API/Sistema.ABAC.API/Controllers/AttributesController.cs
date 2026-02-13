using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para la gestión de atributos del sistema ABAC.
/// Proporciona endpoints para operaciones CRUD de definiciones de atributos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos los endpoints requieren autenticación
public class AttributesController : ControllerBase
{
    private readonly IAttributeService _attributeService;
    private readonly ILogger<AttributesController> _logger;

    /// <summary>
    /// Constructor del controlador de atributos.
    /// </summary>
    public AttributesController(IAttributeService attributeService, ILogger<AttributesController> logger)
    {
        _attributeService = attributeService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una lista paginada de atributos con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
    /// <param name="searchTerm">Término de búsqueda para filtrar por nombre, clave o descripción</param>
    /// <param name="type">Filtrar por tipo de atributo (String, Number, Boolean, DateTime)</param>
    /// <param name="sortBy">Campo para ordenar: Name, Key, Type, CreatedAt (por defecto Name)</param>
    /// <param name="sortDescending">Orden descendente (por defecto false)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de atributos</returns>
    /// <response code="200">Lista de atributos obtenida exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<AttributeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<AttributeDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? type = null,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo atributos: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}, Type={Type}",
            page, pageSize, searchTerm, type);

        var result = await _attributeService.GetAllAsync(
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
    /// Obtiene un atributo específico por su ID.
    /// </summary>
    /// <param name="id">ID del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos del atributo</returns>
    /// <response code="200">Atributo encontrado</response>
    /// <response code="404">Atributo no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AttributeDto>> GetById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo atributo con ID: {AttributeId}", id);

        var attribute = await _attributeService.GetByIdAsync(id, cancellationToken);

        if (attribute == null)
        {
            _logger.LogWarning("Atributo no encontrado: {AttributeId}", id);
            return NotFound(new { message = $"Atributo con ID {id} no encontrado" });
        }

        return Ok(attribute);
    }

    /// <summary>
    /// Crea un nuevo atributo en el sistema.
    /// </summary>
    /// <param name="createDto">Datos del atributo a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Atributo creado</returns>
    /// <response code="201">Atributo creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPost]
    [ProducesResponseType(typeof(AttributeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AttributeDto>> Create(
        [FromBody] CreateAttributeDto createDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando nuevo atributo: {AttributeName} ({AttributeKey})", 
            createDto.Name, createDto.Key);

        var attribute = await _attributeService.CreateAsync(createDto, cancellationToken);

        _logger.LogInformation("Atributo creado exitosamente: {AttributeId}", attribute.Id);

        return CreatedAtAction(
            nameof(GetById), 
            new { id = attribute.Id }, 
            attribute);
    }

    /// <summary>
    /// Actualiza un atributo existente.
    /// </summary>
    /// <param name="id">ID del atributo a actualizar</param>
    /// <param name="updateDto">Datos actualizados del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Atributo actualizado</returns>
    /// <response code="200">Atributo actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Atributo no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AttributeDto>> Update(
        Guid id,
        [FromBody] UpdateAttributeDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando atributo: {AttributeId}", id);

        var attribute = await _attributeService.UpdateAsync(id, updateDto, cancellationToken);

        _logger.LogInformation("Atributo actualizado exitosamente: {AttributeId}", id);

        return Ok(attribute);
    }

    /// <summary>
    /// Elimina un atributo del sistema (soft delete).
    /// </summary>
    /// <param name="id">ID del atributo a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Atributo eliminado exitosamente</response>
    /// <response code="404">Atributo no encontrado</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando atributo: {AttributeId}", id);

        await _attributeService.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Atributo eliminado exitosamente: {AttributeId}", id);

        return NoContent();
    }
}
