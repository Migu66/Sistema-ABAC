namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Fachada principal para evaluar solicitudes de acceso mediante ABAC.
/// </summary>
public interface IAccessControlService
{
    /// <summary>
    /// Evalúa si un usuario puede ejecutar una acción sobre un recurso en un contexto dado.
    /// </summary>
    /// <param name="userId">ID del usuario (subject)</param>
    /// <param name="resourceId">ID del recurso (resource)</param>
    /// <param name="actionId">ID de la acción solicitada (action)</param>
    /// <param name="context">Atributos adicionales de contexto (environment)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado detallado de autorización</returns>
    Task<AuthorizationResult> CheckAccessAsync(
        Guid userId,
        Guid resourceId,
        Guid actionId,
        IDictionary<string, object?>? context = null,
        CancellationToken cancellationToken = default);
}