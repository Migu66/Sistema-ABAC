using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz de servicio para la gestión de recursos del sistema ABAC.
/// Proporciona operaciones CRUD para recursos y gestión de sus atributos.
/// </summary>
public interface IResourceService
{
    #region CRUD Operations

    /// <summary>
    /// Obtiene un recurso por su identificador.
    /// </summary>
    /// <param name="resourceId">ID del recurso</param>
    /// <param name="includeAttributes">Indica si se deben incluir los atributos del recurso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO con los datos del recurso</returns>
    Task<ResourceDto?> GetByIdAsync(Guid resourceId, bool includeAttributes = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una lista paginada de recursos con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1)</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="searchTerm">Término de búsqueda (busca en nombre, tipo, descripción)</param>
    /// <param name="type">Filtrar por tipo de recurso</param>
    /// <param name="sortBy">Campo por el cual ordenar (Name, Type, CreatedAt)</param>
    /// <param name="sortDescending">Indica si el orden es descendente</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado con la lista de recursos</returns>
    Task<PagedResultDto<ResourceDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? type = null,
        string sortBy = "Name",
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo recurso en el sistema.
    /// </summary>
    /// <param name="createDto">DTO con los datos del recurso a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del recurso creado</returns>
    Task<ResourceDto> CreateAsync(CreateResourceDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un recurso existente.
    /// </summary>
    /// <param name="resourceId">ID del recurso a actualizar</param>
    /// <param name="updateDto">DTO con los datos a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del recurso actualizado</returns>
    Task<ResourceDto> UpdateAsync(Guid resourceId, UpdateResourceDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un recurso (soft delete).
    /// </summary>
    /// <param name="resourceId">ID del recurso a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se eliminó correctamente</returns>
    Task<bool> DeleteAsync(Guid resourceId, CancellationToken cancellationToken = default);

    #endregion

    #region Resource Attributes Management

    /// <summary>
    /// Obtiene todos los atributos asignados a un recurso.
    /// </summary>
    /// <param name="resourceId">ID del recurso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de atributos del recurso</returns>
    Task<List<ResourceAttributeDto>> GetAttributesAsync(Guid resourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asigna un atributo a un recurso con un valor específico.
    /// </summary>
    /// <param name="resourceId">ID del recurso</param>
    /// <param name="assignDto">DTO con el atributo y valor a asignar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del atributo asignado</returns>
    Task<ResourceAttributeDto> AssignAttributeAsync(Guid resourceId, AssignResourceAttributeDto assignDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el valor de un atributo de un recurso.
    /// </summary>
    /// <param name="resourceId">ID del recurso</param>
    /// <param name="attributeId">ID del atributo</param>
    /// <param name="newValue">Nuevo valor del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del atributo actualizado</returns>
    Task<ResourceAttributeDto> UpdateAttributeAsync(Guid resourceId, Guid attributeId, string newValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remueve un atributo de un recurso.
    /// </summary>
    /// <param name="resourceId">ID del recurso</param>
    /// <param name="attributeId">ID del atributo a remover</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se removió correctamente</returns>
    Task<bool> RemoveAttributeAsync(Guid resourceId, Guid attributeId, CancellationToken cancellationToken = default);

    #endregion
}
