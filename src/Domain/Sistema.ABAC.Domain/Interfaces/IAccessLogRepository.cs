using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para la entidad AccessLog.
/// Maneja el registro y consulta de logs de auditoría del sistema ABAC.
/// </summary>
public interface IAccessLogRepository : IRepository<AccessLog>
{
    /// <summary>
    /// Obtiene logs de acceso filtrados por usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs del usuario</returns>
    Task<IEnumerable<AccessLog>> GetByUserAsync(
        Guid userId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de acceso filtrados por recurso.
    /// </summary>
    /// <param name="resourceId">Identificador del recurso</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs del recurso</returns>
    Task<IEnumerable<AccessLog>> GetByResourceAsync(
        Guid resourceId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de acceso filtrados por acción.
    /// </summary>
    /// <param name="actionId">Identificador de la acción</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs de la acción</returns>
    Task<IEnumerable<AccessLog>> GetByActionAsync(
        Guid actionId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de acceso filtrados por resultado (Permit, Deny, Error).
    /// </summary>
    /// <param name="result">Resultado de la evaluación</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs con el resultado especificado</returns>
    Task<IEnumerable<AccessLog>> GetByResultAsync(
        string result, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de acceso en un rango de fechas.
    /// </summary>
    /// <param name="fromDate">Fecha desde</param>
    /// <param name="toDate">Fecha hasta</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs en el rango de fechas</returns>
    Task<IEnumerable<AccessLog>> GetByDateRangeAsync(
        DateTime fromDate, 
        DateTime toDate, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs con filtros combinados (para búsquedas avanzadas).
    /// </summary>
    /// <param name="userId">Identificador del usuario (opcional)</param>
    /// <param name="resourceId">Identificador del recurso (opcional)</param>
    /// <param name="actionId">Identificador de la acción (opcional)</param>
    /// <param name="result">Resultado de la evaluación (opcional)</param>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs que cumplen los filtros</returns>
    Task<IEnumerable<AccessLog>> GetWithFiltersAsync(
        Guid? userId = null,
        Guid? resourceId = null,
        Guid? actionId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de acceso: total de intentos, permitidos, denegados y errores.
    /// </summary>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas agregadas de acceso</returns>
    Task<AccessLogStatistics> GetStatisticsAsync(
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los recursos más accedidos en un período.
    /// </summary>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="top">Número de recursos a devolver (por defecto 10)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de recursos con su número de accesos</returns>
    Task<IEnumerable<(Guid ResourceId, int AccessCount)>> GetMostAccessedResourcesAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int top = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los usuarios más activos en un período.
    /// </summary>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="top">Número de usuarios a devolver (por defecto 10)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de usuarios con su número de intentos de acceso</returns>
    Task<IEnumerable<(Guid UserId, int AccessCount)>> GetMostActiveUsersAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int top = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene accesos denegados agrupados por política (para análisis de seguridad).
    /// </summary>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas con su número de denegaciones</returns>
    Task<IEnumerable<(Guid PolicyId, int DenialCount)>> GetDenialsByPolicyAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Clase para almacenar estadísticas agregadas de logs de acceso.
/// </summary>
public class AccessLogStatistics
{
    /// <summary>
    /// Total de intentos de acceso registrados.
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Número de accesos permitidos.
    /// </summary>
    public int PermittedAccess { get; set; }

    /// <summary>
    /// Número de accesos denegados.
    /// </summary>
    public int DeniedAccess { get; set; }

    /// <summary>
    /// Número de errores durante evaluación.
    /// </summary>
    public int Errors { get; set; }

    /// <summary>
    /// Porcentaje de accesos permitidos.
    /// </summary>
    public double PermitRate => TotalAttempts > 0 ? (double)PermittedAccess / TotalAttempts * 100 : 0;

    /// <summary>
    /// Porcentaje de accesos denegados.
    /// </summary>
    public double DenyRate => TotalAttempts > 0 ? (double)DeniedAccess / TotalAttempts * 100 : 0;

    /// <summary>
    /// Porcentaje de errores.
    /// </summary>
    public double ErrorRate => TotalAttempts > 0 ? (double)Errors / TotalAttempts * 100 : 0;
}
