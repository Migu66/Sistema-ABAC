using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para la entidad Resource.
/// Maneja los recursos protegidos por el sistema ABAC.
/// </summary>
public interface IResourceRepository : IRepository<Resource>
{
    /// <summary>
    /// Obtiene un recurso con todos sus atributos ABAC incluidos.
    /// </summary>
    /// <param name="resourceId">Identificador del recurso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Recurso con sus atributos o null</returns>
    Task<Resource?> GetWithAttributesAsync(Guid resourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los recursos de un tipo específico.
    /// </summary>
    /// <param name="type">Tipo de recurso (ej: "documento", "endpoint", "vista")</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de recursos del tipo especificado</returns>
    Task<IEnumerable<Resource>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene recursos que tengan un atributo específico con un valor determinado.
    /// Útil para búsquedas ABAC (ej: "encontrar todos los recursos clasificados como Confidencial").
    /// </summary>
    /// <param name="attributeKey">Clave del atributo a buscar</param>
    /// <param name="attributeValue">Valor del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de recursos que tienen el atributo con el valor especificado</returns>
    Task<IEnumerable<Resource>> GetByAttributeAsync(
        string attributeKey, 
        string attributeValue, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca recursos por nombre (búsqueda parcial, case-insensitive).
    /// </summary>
    /// <param name="searchTerm">Término de búsqueda</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de recursos que coinciden con el término de búsqueda</returns>
    Task<IEnumerable<Resource>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el historial de acceso (logs) de un recurso con paginación.
    /// </summary>
    /// <param name="resourceId">Identificador del recurso</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs de acceso al recurso</returns>
    Task<IEnumerable<AccessLog>> GetAccessLogsAsync(
        Guid resourceId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);
}
