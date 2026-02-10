using Sistema.ABAC.Domain.Common;
using System.Linq.Expressions;

namespace Sistema.ABAC.Domain.Interfaces;

/// <summary>
/// Interfaz genérica base para repositorios con operaciones CRUD estándar.
/// Define el contrato para acceso a datos de todas las entidades que heredan de BaseEntity.
/// </summary>
/// <typeparam name="T">Tipo de entidad que hereda de BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Obtiene una entidad por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la entidad</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La entidad encontrada o null si no existe</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las entidades del repositorio.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de todas las entidades</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca entidades que cumplan con el predicado especificado.
    /// </summary>
    /// <param name="predicate">Expresión lambda para filtrar entidades</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de entidades que cumplen el criterio</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la primera entidad que cumpla con el predicado o null si no existe.
    /// </summary>
    /// <param name="predicate">Expresión lambda para filtrar entidades</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La primera entidad que cumple el criterio o null</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega una nueva entidad al repositorio.
    /// </summary>
    /// <param name="entity">Entidad a agregar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La entidad agregada con su ID generado</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega múltiples entidades al repositorio.
    /// </summary>
    /// <param name="entities">Colección de entidades a agregar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    /// <param name="entity">Entidad con los datos actualizados</param>
    void Update(T entity);

    /// <summary>
    /// Actualiza múltiples entidades.
    /// </summary>
    /// <param name="entities">Colección de entidades a actualizar</param>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Elimina una entidad del repositorio (soft delete si IsDeleted está disponible).
    /// </summary>
    /// <param name="entity">Entidad a eliminar</param>
    void Remove(T entity);

    /// <summary>
    /// Elimina múltiples entidades del repositorio.
    /// </summary>
    /// <param name="entities">Colección de entidades a eliminar</param>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Verifica si existe alguna entidad que cumpla con el predicado.
    /// </summary>
    /// <param name="predicate">Expresión lambda para filtrar entidades</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si existe al menos una entidad que cumple el criterio</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta el número de entidades que cumplen con el predicado.
    /// </summary>
    /// <param name="predicate">Expresión lambda para filtrar entidades</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades que cumplen el criterio</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
