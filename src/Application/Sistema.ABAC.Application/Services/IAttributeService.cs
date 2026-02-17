using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz de servicio para la gestión de atributos del sistema ABAC.
/// Proporciona operaciones CRUD para definiciones de atributos.
/// </summary>
public interface IAttributeService
{
    #region CRUD Operations

    /// <summary>
    /// Obtiene un atributo por su identificador.
    /// </summary>
    /// <param name="attributeId">ID del atributo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO con los datos del atributo</returns>
    Task<AttributeDto?> GetByIdAsync(Guid attributeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una lista paginada de atributos con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1)</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="searchTerm">Término de búsqueda (busca en nombre, clave, descripción)</param>
    /// <param name="type">Filtrar por tipo de atributo</param>
    /// <param name="sortBy">Campo por el cual ordenar (Name, Key, Type, CreatedAt)</param>
    /// <param name="sortDescending">Indica si el orden es descendente</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado con la lista de atributos</returns>
    Task<PagedResultDto<AttributeDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? type = null,
        string sortBy = "Name",
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo atributo en el sistema.
    /// </summary>
    /// <param name="createDto">DTO con los datos del atributo a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del atributo creado</returns>
    Task<AttributeDto> CreateAsync(CreateAttributeDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un atributo existente.
    /// </summary>
    /// <param name="attributeId">ID del atributo a actualizar</param>
    /// <param name="updateDto">DTO con los datos a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO del atributo actualizado</returns>
    Task<AttributeDto> UpdateAsync(Guid attributeId, UpdateAttributeDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un atributo (soft delete).
    /// </summary>
    /// <param name="attributeId">ID del atributo a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se eliminó correctamente</returns>
    Task<bool> DeleteAsync(Guid attributeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un atributo con la clave especificada.
    /// </summary>
    /// <param name="key">Clave del atributo</param>
    /// <param name="excludeId">ID del atributo a excluir de la búsqueda (útil para validar al actualizar)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si existe un atributo con esa clave</returns>
    Task<bool> ExistsByKeyAsync(string key, Guid? excludeId = null, CancellationToken cancellationToken = default);

    #endregion
}
