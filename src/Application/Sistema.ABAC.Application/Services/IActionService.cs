using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz de servicio para la gestión de acciones del sistema ABAC.
/// Proporciona operaciones CRUD para acciones que pueden realizarse sobre recursos.
/// </summary>
public interface IActionService
{
    #region CRUD Operations

    /// <summary>
    /// Obtiene una acción por su identificador.
    /// </summary>
    /// <param name="actionId">ID de la acción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO con los datos de la acción</returns>
    Task<ActionDto?> GetByIdAsync(Guid actionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una acción por su código único.
    /// </summary>
    /// <param name="code">Código único de la acción (ej: "read", "delete")</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO con los datos de la acción</returns>
    Task<ActionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una lista paginada de acciones con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1)</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="searchTerm">Término de búsqueda (busca en nombre, código, descripción)</param>
    /// <param name="sortBy">Campo por el cual ordenar (Name, Code, CreatedAt)</param>
    /// <param name="sortDescending">Indica si el orden es descendente</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado con la lista de acciones</returns>
    Task<PagedResultDto<ActionDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string sortBy = "Name",
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea una nueva acción en el sistema.
    /// </summary>
    /// <param name="createDto">DTO con los datos de la acción a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO de la acción creada</returns>
    Task<ActionDto> CreateAsync(CreateActionDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una acción existente.
    /// </summary>
    /// <param name="actionId">ID de la acción a actualizar</param>
    /// <param name="updateDto">DTO con los datos actualizados</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO de la acción actualizada</returns>
    Task<ActionDto> UpdateAsync(Guid actionId, UpdateActionDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una acción del sistema (soft delete).
    /// </summary>
    /// <param name="actionId">ID de la acción a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la eliminación fue exitosa</returns>
    Task<bool> DeleteAsync(Guid actionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una acción con el código especificado.
    /// </summary>
    /// <param name="code">Código a verificar</param>
    /// <param name="excludeActionId">ID de acción a excluir de la verificación (útil para updates)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el código ya existe</returns>
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeActionId = null, CancellationToken cancellationToken = default);

    #endregion
}
