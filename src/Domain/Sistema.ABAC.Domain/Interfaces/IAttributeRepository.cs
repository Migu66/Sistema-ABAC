using Sistema.ABAC.Domain.Entities;
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;

namespace Sistema.ABAC.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para la entidad Attribute.
/// Maneja las definiciones de atributos que se pueden asignar a usuarios y recursos.
/// </summary>
public interface IAttributeRepository : IRepository<AttributeEntity>
{
    /// <summary>
    /// Obtiene un atributo por su clave única.
    /// </summary>
    /// <param name="key">Clave técnica del atributo (ej: "departamento", "nivel_acceso")</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Atributo encontrado o null</returns>
    Task<AttributeEntity?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un atributo con todas sus asignaciones a usuarios.
    /// </summary>
    /// <param name="attributeId">Identificador del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Atributo con sus asignaciones a usuarios</returns>
    Task<AttributeEntity?> GetWithUserAttributesAsync(Guid attributeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un atributo con todas sus asignaciones a recursos.
    /// </summary>
    /// <param name="attributeId">Identificador del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Atributo con sus asignaciones a recursos</returns>
    Task<AttributeEntity?> GetWithResourceAttributesAsync(Guid attributeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si una clave de atributo ya existe en el sistema.
    /// </summary>
    /// <param name="key">Clave del atributo a verificar</param>
    /// <param name="excludeId">ID del atributo a excluir de la verificación (útil para updates)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la clave ya existe</returns>
    Task<bool> KeyExistsAsync(string key, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los atributos usados en condiciones de políticas.
    /// Útil para análisis y reportes de qué atributos son críticos para el sistema ABAC.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de atributos que están siendo usados en políticas</returns>
    Task<IEnumerable<AttributeEntity>> GetAttributesUsedInPoliciesAsync(CancellationToken cancellationToken = default);
}
