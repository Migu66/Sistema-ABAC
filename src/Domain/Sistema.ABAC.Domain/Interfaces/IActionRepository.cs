using Sistema.ABAC.Domain.Entities;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para la entidad Action.
/// Maneja las acciones que se pueden realizar sobre recursos.
/// </summary>
public interface IActionRepository : IRepository<ActionEntity>
{
    /// <summary>
    /// Obtiene una acción por su código único.
    /// </summary>
    /// <param name="code">Código de la acción (ej: "read", "write", "delete")</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Acción encontrada o null</returns>
    Task<ActionEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una acción con todas las políticas asociadas.
    /// </summary>
    /// <param name="actionId">Identificador de la acción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Acción con sus políticas asociadas</returns>
    Task<ActionEntity?> GetWithPoliciesAsync(Guid actionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un código de acción ya existe en el sistema.
    /// </summary>
    /// <param name="code">Código de la acción a verificar</param>
    /// <param name="excludeId">ID de la acción a excluir de la verificación (útil para updates)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el código ya existe</returns>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las acciones que están cubiertas por al menos una política activa.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de acciones con políticas activas</returns>
    Task<IEnumerable<ActionEntity>> GetActionsWithActivePoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el historial de intentos de realizar una acción específica.
    /// </summary>
    /// <param name="actionId">Identificador de la acción</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs de la acción</returns>
    Task<IEnumerable<AccessLog>> GetAccessLogsAsync(
        Guid actionId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);
}
