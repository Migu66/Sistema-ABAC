using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para la entidad Policy.
/// Maneja las políticas ABAC del sistema, incluyendo sus condiciones y acciones asociadas.
/// </summary>
public interface IPolicyRepository : IRepository<Policy>
{
    /// <summary>
    /// Obtiene una política con todas sus condiciones y acciones asociadas incluidas.
    /// </summary>
    /// <param name="policyId">Identificador de la política</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Política con sus relaciones completas o null</returns>
    Task<Policy?> GetWithDetailsAsync(Guid policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las políticas activas del sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas activas</returns>
    Task<IEnumerable<Policy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las políticas activas que aplican a una acción específica.
    /// Este es uno de los métodos más importantes para el motor de evaluación ABAC.
    /// </summary>
    /// <param name="actionId">Identificador de la acción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas activas que cubren la acción especificada</returns>
    Task<IEnumerable<Policy>> GetActivePoliciesForActionAsync(
        Guid actionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las políticas activas ordenadas por prioridad (descendente).
    /// Usado por el motor de evaluación para procesar políticas en orden correcto.
    /// </summary>
    /// <param name="actionId">Identificador de la acción (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas ordenadas por prioridad</returns>
    Task<IEnumerable<Policy>> GetPoliciesByPriorityAsync(
        Guid? actionId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene políticas por efecto (Permit o Deny).
    /// </summary>
    /// <param name="effect">Efecto de la política</param>
    /// <param name="activeOnly">Si true, solo devuelve políticas activas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas con el efecto especificado</returns>
    Task<IEnumerable<Policy>> GetByEffectAsync(
        PolicyEffect effect, 
        bool activeOnly = true, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Búsqueda de políticas por nombre (parcial, case-insensitive).
    /// </summary>
    /// <param name="searchTerm">Término de búsqueda</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas que coinciden con el término</returns>
    Task<IEnumerable<Policy>> SearchByNameAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene políticas que usan un atributo específico en sus condiciones.
    /// Útil para análisis de impacto al modificar o eliminar un atributo.
    /// </summary>
    /// <param name="attributeKey">Clave del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas que evalúan el atributo especificado</returns>
    Task<IEnumerable<Policy>> GetPoliciesUsingAttributeAsync(
        string attributeKey, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta cuántas políticas están activas vs inactivas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tupla con (políticas activas, políticas inactivas)</returns>
    Task<(int Active, int Inactive)> GetPolicyStatusCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de aplicación de una política (cuántas veces se ha aplicado).
    /// </summary>
    /// <param name="policyId">Identificador de la política</param>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de veces que la política ha sido aplicada</returns>
    Task<int> GetPolicyApplicationCountAsync(
        Guid policyId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default);
}
