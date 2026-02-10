using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio para la entidad User.
/// Proporciona operaciones específicas para usuarios.
/// </summary>
/// <remarks>
/// User utiliza Identity, por lo que muchas operaciones se manejan a través de UserManager.
/// Este repositorio se enfoca en operaciones relacionadas con ABAC (atributos, logs, etc.).
/// NOTA: User no hereda de BaseEntity sino de IdentityUser, por lo que no extiende IRepository.
/// </remarks>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El usuario encontrado o null si no existe</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los usuarios del sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de todos los usuarios</returns>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega un nuevo usuario al sistema.
    /// </summary>
    /// <param name="user">Usuario a agregar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El usuario agregado</returns>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    /// <param name="user">Usuario con los datos actualizados</param>
    void Update(User user);

    /// <summary>
    /// Elimina un usuario del sistema (soft delete).
    /// </summary>
    /// <param name="user">Usuario a eliminar</param>
    void Remove(User user);

    /// <summary>
    /// Obtiene un usuario por su dirección de correo electrónico.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario encontrado o null</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario por su nombre de usuario.
    /// </summary>
    /// <param name="userName">Nombre de usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario encontrado o null</returns>
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario con todos sus atributos ABAC incluidos.
    /// </summary>
    /// <param name="userId">Identificador del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario con sus atributos o null</returns>
    Task<User?> GetWithAttributesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los atributos activos de un usuario en una fecha específica.
    /// Considera ValidFrom y ValidTo de UserAttribute.
    /// </summary>
    /// <param name="userId">Identificador del usuario</param>
    /// <param name="evaluationDate">Fecha para evaluar validez de atributos (por defecto DateTime.UtcNow)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de atributos del usuario válidos en la fecha especificada</returns>
    Task<IEnumerable<UserAttribute>> GetActiveAttributesAsync(
        Guid userId, 
        DateTime? evaluationDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene usuarios que tengan un atributo específico con un valor determinado.
    /// Útil para consultas ABAC inversas (ej: "encontrar todos los usuarios del departamento Ventas").
    /// </summary>
    /// <param name="attributeKey">Clave del atributo a buscar</param>
    /// <param name="attributeValue">Valor del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de usuarios que tienen el atributo con el valor especificado</returns>
    Task<IEnumerable<User>> GetByAttributeAsync(
        string attributeKey, 
        string attributeValue, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el historial de acceso (logs) de un usuario con paginación.
    /// </summary>
    /// <param name="userId">Identificador del usuario</param>
    /// <param name="skip">Número de registros a omitir</param>
    /// <param name="take">Número de registros a tomar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de logs de acceso del usuario</returns>
    Task<IEnumerable<AccessLog>> GetAccessLogsAsync(
        Guid userId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);
}
