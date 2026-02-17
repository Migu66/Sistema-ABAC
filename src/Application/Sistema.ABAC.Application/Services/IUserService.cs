using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.DTOs.Common;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz de servicio para la gestión de usuarios en el sistema ABAC.
/// Proporciona operaciones CRUD y gestión de atributos de usuario.
/// </summary>
public interface IUserService
{
    #region CRUD Operations

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="includeAttributes">Indica si se deben incluir los atributos del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO con los datos del usuario</returns>
    Task<UserDto?> GetByIdAsync(Guid userId, bool includeAttributes = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1)</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="searchTerm">Término de búsqueda (busca en nombre, email, username)</param>
    /// <param name="department">Filtrar por departamento</param>
    /// <param name="isActive">Filtrar por estado activo/inactivo</param>
    /// <param name="sortBy">Campo por el cual ordenar (UserName, Email, FullName, CreatedAt)</param>
    /// <param name="sortDescending">Indica si el orden es descendente</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado con la lista de usuarios</returns>
    Task<PagedResultDto<UserDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? department = null,
        bool? isActive = null,
        string sortBy = "UserName",
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <param name="userId">ID del usuario a actualizar</param>
    /// <param name="updateDto">DTO con los datos a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del usuario actualizado</returns>
    Task<UserDto> UpdateAsync(Guid userId, UpdateUserDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un usuario del sistema (soft delete).
    /// </summary>
    /// <param name="userId">ID del usuario a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    #endregion

    #region User Attributes Management

    /// <summary>
    /// Obtiene todos los atributos asignados a un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="includeExpired">Indica si se deben incluir atributos expirados</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de atributos del usuario</returns>
    Task<IEnumerable<UserAttributeDto>> GetUserAttributesAsync(
        Guid userId,
        bool includeExpired = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asigna un atributo a un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="assignDto">DTO con los datos del atributo a asignar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del atributo asignado</returns>
    Task<UserAttributeDto> AssignAttributeAsync(
        Guid userId,
        AssignUserAttributeDto assignDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el valor de un atributo asignado a un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="attributeId">ID del atributo</param>
    /// <param name="updateDto">DTO con los datos actualizados del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del atributo actualizado</returns>
    Task<UserAttributeDto> UpdateAttributeAsync(
        Guid userId,
        Guid attributeId,
        UpdateUserAttributeDto updateDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remueve un atributo de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="attributeId">ID del atributo a remover</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task RemoveAttributeAsync(Guid userId, Guid attributeId, CancellationToken cancellationToken = default);

    #endregion

    #region Query Methods

    /// <summary>
    /// Obtiene usuarios que tienen un atributo específico con un valor determinado.
    /// </summary>
    /// <param name="attributeKey">Clave del atributo</param>
    /// <param name="attributeValue">Valor del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de usuarios con el atributo especificado</returns>
    Task<IEnumerable<UserDto>> GetUsersByAttributeAsync(
        string attributeKey,
        string attributeValue,
        CancellationToken cancellationToken = default);

    #endregion
}

