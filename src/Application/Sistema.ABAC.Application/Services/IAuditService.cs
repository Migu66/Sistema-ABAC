using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz del servicio de auditoría para registrar y consultar evaluaciones de acceso ABAC.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra una evaluación de acceso en el log de auditoría.
    /// </summary>
    /// <param name="userId">ID del usuario evaluado</param>
    /// <param name="resourceId">ID del recurso evaluado</param>
    /// <param name="actionId">ID de la acción evaluada</param>
    /// <param name="result">Resultado de la evaluación (Permit, Deny, Error, etc.)</param>
    /// <param name="reason">Razón detallada de la decisión</param>
    /// <param name="policyId">ID de la política que determinó la decisión final</param>
    /// <param name="context">Contexto adicional serializado o textual</param>
    /// <param name="ipAddress">IP origen de la solicitud</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Log registrado</returns>
    Task<AccessLogDto> LogAccessEvaluationAsync(
        Guid userId,
        Guid? resourceId,
        Guid? actionId,
        string result,
        string? reason = null,
        Guid? policyId = null,
        string? context = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de auditoría con filtros y paginación.
    /// </summary>
    /// <param name="filter">Filtros de búsqueda</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado de logs</returns>
    Task<PagedResultDto<AccessLogDto>> GetLogsAsync(
        AccessLogFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas agregadas de accesos.
    /// </summary>
    /// <param name="fromDate">Fecha inicial opcional</param>
    /// <param name="toDate">Fecha final opcional</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de acceso</returns>
    Task<AccessLogStatisticsDto> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}