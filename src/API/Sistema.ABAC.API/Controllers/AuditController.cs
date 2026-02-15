using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para la consulta de logs y estadísticas de auditoría ABAC.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    /// <summary>
    /// Constructor del controlador de auditoría.
    /// </summary>
    public AuditController(
        IAuditService auditService,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene logs de auditoría con filtros opcionales.
    /// </summary>
    /// <param name="filter">Filtros de búsqueda: userId, resourceId, actionId, result, fromDate, toDate, page y pageSize</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado de logs de auditoría</returns>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(PagedResultDto<AccessLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<AccessLogDto>>> GetLogs(
        [FromQuery] AccessLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Consultando logs de auditoría: UserId={UserId}, ResourceId={ResourceId}, ActionId={ActionId}, Result={Result}, FromDate={FromDate}, ToDate={ToDate}, Page={Page}, PageSize={PageSize}",
            filter.UserId,
            filter.ResourceId,
            filter.ActionId,
            filter.Result,
            filter.FromDate,
            filter.ToDate,
            filter.Page,
            filter.PageSize);

        var result = await _auditService.GetLogsAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estadísticas agregadas de auditoría.
    /// </summary>
    /// <param name="fromDate">Fecha inicial opcional para filtrar estadísticas</param>
    /// <param name="toDate">Fecha final opcional para filtrar estadísticas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Conteos y tasas de accesos permitidos, denegados y errores</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(AccessLogStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessLogStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Consultando estadísticas de auditoría: FromDate={FromDate}, ToDate={ToDate}",
            fromDate,
            toDate);

        var statistics = await _auditService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(statistics);
    }
}