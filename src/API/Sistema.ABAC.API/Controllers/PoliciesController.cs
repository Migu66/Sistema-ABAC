using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para la gestión de políticas ABAC del sistema.
/// Proporciona endpoints completos para operaciones CRUD de políticas, gestión de condiciones y acciones asociadas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos los endpoints requieren autenticación
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;
    private readonly ILogger<PoliciesController> _logger;

    /// <summary>
    /// Constructor del controlador de políticas.
    /// </summary>
    public PoliciesController(IPolicyService policyService, ILogger<PoliciesController> logger)
    {
        _policyService = policyService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Obtiene una lista paginada de políticas con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
    /// <param name="searchTerm">Término de búsqueda para filtrar por nombre o descripción</param>
    /// <param name="effect">Filtrar por efecto: Permit o Deny</param>
    /// <param name="isActive">Filtrar por estado: true (activas), false (inactivas), null (todas)</param>
    /// <param name="sortBy">Campo para ordenar: Name, Priority, CreatedAt (por defecto Priority)</param>
    /// <param name="sortDescending">Orden descendente (por defecto true para prioridad más alta primero)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de políticas</returns>
    /// <response code="200">Lista de políticas obtenida exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<PolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<PolicyDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] PolicyEffect? effect = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string sortBy = "Priority",
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo políticas: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}, Effect={Effect}, IsActive={IsActive}",
            page, pageSize, searchTerm, effect, isActive);

        var result = await _policyService.GetAllAsync(
            page, 
            pageSize, 
            searchTerm,
            effect,
            isActive,
            sortBy, 
            sortDescending, 
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene una política específica por su ID.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="includeDetails">Incluir condiciones y acciones asociadas (por defecto true)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos de la política</returns>
    /// <response code="200">Política encontrada</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PolicyDto>> GetById(
        Guid id,
        [FromQuery] bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo política con ID: {PolicyId}, incluir detalles: {IncludeDetails}", 
            id, includeDetails);

        var policy = await _policyService.GetByIdAsync(id, includeDetails, cancellationToken);

        if (policy == null)
        {
            _logger.LogWarning("Política no encontrada: {PolicyId}", id);
            return NotFound(new { message = $"Política con ID {id} no encontrada" });
        }

        return Ok(policy);
    }

    /// <summary>
    /// Obtiene todas las políticas activas del sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas activas</returns>
    /// <response code="200">Lista de políticas activas obtenida exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// Este endpoint devuelve solo las políticas que están activas y serán evaluadas por el motor ABAC.
    /// </remarks>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<PolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<PolicyDto>>> GetActive(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo todas las políticas activas");

        var policies = await _policyService.GetActivePoliciesAsync(cancellationToken);

        return Ok(policies);
    }

    /// <summary>
    /// Obtiene todas las políticas activas que aplican a una acción específica.
    /// </summary>
    /// <param name="actionId">ID de la acción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas que cubren la acción</returns>
    /// <response code="200">Lista de políticas obtenida exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// Útil para ver qué políticas se evaluarán cuando un usuario intente realizar una acción específica.
    /// </remarks>
    [HttpGet("by-action/{actionId:guid}")]
    [ProducesResponseType(typeof(List<PolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<PolicyDto>>> GetByAction(
        Guid actionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo políticas para acción: {ActionId}", actionId);

        var policies = await _policyService.GetPoliciesForActionAsync(actionId, cancellationToken);

        return Ok(policies);
    }

    /// <summary>
    /// Crea una nueva política en el sistema.
    /// </summary>
    /// <param name="createDto">Datos de la política a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Política creada</returns>
    /// <response code="201">Política creada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// Puede crear la política con condiciones y acciones incluidas en una sola operación.
    /// Por defecto se crea activa, pero puede crearla inactiva para configurarla después.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PolicyDto>> Create(
        [FromBody] CreatePolicyDto createDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando nueva política: {PolicyName} (Efecto: {Effect}, Prioridad: {Priority})", 
            createDto.Name, createDto.Effect, createDto.Priority);

        var policy = await _policyService.CreateAsync(createDto, cancellationToken);

        _logger.LogInformation("Política creada exitosamente: {PolicyId}", policy.Id);

        return CreatedAtAction(
            nameof(GetById), 
            new { id = policy.Id }, 
            policy);
    }

    /// <summary>
    /// Actualiza una política existente.
    /// </summary>
    /// <param name="id">ID de la política a actualizar</param>
    /// <param name="updateDto">Datos actualizados de la política</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Política actualizada</returns>
    /// <response code="200">Política actualizada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// Este endpoint actualiza solo la información básica de la política.
    /// Para gestionar condiciones y acciones, use los endpoints específicos.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PolicyDto>> Update(
        Guid id,
        [FromBody] UpdatePolicyDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando política: {PolicyId}", id);

        var policy = await _policyService.UpdateAsync(id, updateDto, cancellationToken);

        _logger.LogInformation("Política actualizada exitosamente: {PolicyId}", id);

        return Ok(policy);
    }

    /// <summary>
    /// Elimina una política del sistema (soft delete).
    /// </summary>
    /// <param name="id">ID de la política a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Política eliminada exitosamente</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// El sistema realiza un soft delete. Los registros históricos y logs se mantienen.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando política: {PolicyId}", id);

        await _policyService.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Política eliminada exitosamente: {PolicyId}", id);

        return NoContent();
    }

    #endregion

    #region Policy Activation

    /// <summary>
    /// Activa una política para que sea evaluada por el motor ABAC.
    /// </summary>
    /// <param name="id">ID de la política a activar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Política activada</returns>
    /// <response code="200">Política activada exitosamente</response>
    /// <response code="400">La política no puede ser activada (falta configuración)</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// El sistema valida que la política tenga al menos una condición y una acción antes de activarla.
    /// </remarks>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PolicyDto>> Activate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activando política: {PolicyId}", id);

        var policy = await _policyService.ActivateAsync(id, cancellationToken);

        _logger.LogInformation("Política activada exitosamente: {PolicyId}", id);

        return Ok(policy);
    }

    /// <summary>
    /// Desactiva una política para que no sea evaluada por el motor ABAC.
    /// </summary>
    /// <param name="id">ID de la política a desactivar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Política desactivada</returns>
    /// <response code="200">Política desactivada exitosamente</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// Útil para deshabilitar temporalmente una política sin eliminarla. Ideal para mantenimiento o pruebas.
    /// </remarks>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PolicyDto>> Deactivate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Desactivando política: {PolicyId}", id);

        var policy = await _policyService.DeactivateAsync(id, cancellationToken);

        _logger.LogInformation("Política desactivada exitosamente: {PolicyId}", id);

        return Ok(policy);
    }

    #endregion

    #region Condition Management

    /// <summary>
    /// Obtiene todas las condiciones de una política.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de condiciones</returns>
    /// <response code="200">Lista de condiciones obtenida exitosamente</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}/conditions")]
    [ProducesResponseType(typeof(List<PolicyConditionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<PolicyConditionDto>>> GetConditions(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo condiciones de política: {PolicyId}", id);

        var conditions = await _policyService.GetConditionsAsync(id, cancellationToken);

        return Ok(conditions);
    }

    /// <summary>
    /// Añade una nueva condición a una política.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="createDto">Datos de la condición a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Condición creada</returns>
    /// <response code="201">Condición añadida exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPost("{id:guid}/conditions")]
    [ProducesResponseType(typeof(PolicyConditionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PolicyConditionDto>> AddCondition(
        Guid id,
        [FromBody] CreatePolicyConditionDto createDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Añadiendo condición a política: {PolicyId}", id);

        var condition = await _policyService.AddConditionAsync(id, createDto, cancellationToken);

        _logger.LogInformation("Condición añadida exitosamente a política: {PolicyId}", id);

        return CreatedAtAction(
            nameof(GetConditions),
            new { id },
            condition);
    }

    /// <summary>
    /// Actualiza una condición existente de una política.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="conditionId">ID de la condición a actualizar</param>
    /// <param name="updateDto">Datos actualizados de la condición</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Condición actualizada</returns>
    /// <response code="200">Condición actualizada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Política o condición no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPut("{id:guid}/conditions/{conditionId:guid}")]
    [ProducesResponseType(typeof(PolicyConditionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PolicyConditionDto>> UpdateCondition(
        Guid id,
        Guid conditionId,
        [FromBody] UpdatePolicyConditionDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando condición {ConditionId} de política {PolicyId}", conditionId, id);

        var condition = await _policyService.UpdateConditionAsync(id, conditionId, updateDto, cancellationToken);

        _logger.LogInformation("Condición actualizada exitosamente: {ConditionId}", conditionId);

        return Ok(condition);
    }

    /// <summary>
    /// Elimina una condición de una política.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="conditionId">ID de la condición a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Condición eliminada exitosamente</response>
    /// <response code="400">No se puede eliminar la única condición de una política activa</response>
    /// <response code="404">Política o condición no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpDelete("{id:guid}/conditions/{conditionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveCondition(
        Guid id,
        Guid conditionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando condición {ConditionId} de política {PolicyId}", conditionId, id);

        await _policyService.RemoveConditionAsync(id, conditionId, cancellationToken);

        _logger.LogInformation("Condición eliminada exitosamente: {ConditionId}", conditionId);

        return NoContent();
    }

    #endregion

    #region Action Association Management

    /// <summary>
    /// Obtiene todas las acciones asociadas a una política.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de acciones</returns>
    /// <response code="200">Lista de acciones obtenida exitosamente</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}/actions")]
    [ProducesResponseType(typeof(List<ActionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ActionDto>>> GetActions(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo acciones de política: {PolicyId}", id);

        var actions = await _policyService.GetActionsAsync(id, cancellationToken);

        return Ok(actions);
    }

    /// <summary>
    /// Asocia una o más acciones a una política.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="actionIds">IDs de las acciones a asociar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Acciones asociadas exitosamente</response>
    /// <response code="400">IDs de acciones inválidos</response>
    /// <response code="404">Política o alguna acción no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPost("{id:guid}/actions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssociateActions(
        Guid id,
        [FromBody] List<Guid> actionIds,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Asociando {Count} acciones a política: {PolicyId}", actionIds.Count, id);

        await _policyService.AssociateActionsAsync(id, actionIds, cancellationToken);

        _logger.LogInformation("Acciones asociadas exitosamente a política: {PolicyId}", id);

        return NoContent();
    }

    /// <summary>
    /// Desasocia una acción de una política.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="actionId">ID de la acción a desasociar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Acción desasociada exitosamente</response>
    /// <response code="400">No se puede desasociar la única acción de una política activa</response>
    /// <response code="404">Política o acción no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpDelete("{id:guid}/actions/{actionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DisassociateAction(
        Guid id,
        Guid actionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Desasociando acción {ActionId} de política {PolicyId}", actionId, id);

        await _policyService.DisassociateActionAsync(id, actionId, cancellationToken);

        _logger.LogInformation("Acción desasociada exitosamente: {ActionId}", actionId);

        return NoContent();
    }

    #endregion

    #region Validation and Statistics

    /// <summary>
    /// Valida si una política tiene una configuración correcta.
    /// </summary>
    /// <param name="id">ID de la política a validar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la validación</returns>
    /// <response code="200">Validación completada</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <remarks>
    /// Una política válida debe tener al menos una condición y una acción asociada.
    /// </remarks>
    [HttpGet("{id:guid}/validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<bool>> Validate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validando política: {PolicyId}", id);

        var isValid = await _policyService.ValidatePolicyAsync(id, cancellationToken);

        return Ok(new { isValid, policyId = id });
    }

    /// <summary>
    /// Obtiene estadísticas detalladas de una política.
    /// </summary>
    /// <param name="id">ID de la política</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de la política</returns>
    /// <response code="200">Estadísticas obtenidas exitosamente</response>
    /// <response code="404">Política no encontrada</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("{id:guid}/statistics")]
    [ProducesResponseType(typeof(PolicyStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PolicyStatisticsDto>> GetStatistics(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo estadísticas de política: {PolicyId}", id);

        var statistics = await _policyService.GetStatisticsAsync(id, cancellationToken);

        return Ok(statistics);
    }

    #endregion
}
