using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz de servicio para la gestión de políticas ABAC del sistema.
/// Proporciona operaciones CRUD para políticas y gestión de sus condiciones y acciones asociadas.
/// </summary>
public interface IPolicyService
{
    #region CRUD Operations

    /// <summary>
    /// Obtiene una política por su identificador.
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="includeDetails">Indica si se deben incluir condiciones y acciones asociadas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO con los datos de la política</returns>
    Task<PolicyDto?> GetByIdAsync(Guid policyId, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una lista paginada de políticas con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1)</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="searchTerm">Término de búsqueda (busca en nombre y descripción)</param>
    /// <param name="effect">Filtrar por efecto (Permit, Deny)</param>
    /// <param name="isActive">Filtrar por estado activo/inactivo (null = todos)</param>
    /// <param name="sortBy">Campo por el cual ordenar (Name, Priority, CreatedAt)</param>
    /// <param name="sortDescending">Indica si el orden es descendente</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado con la lista de políticas</returns>
    Task<PagedResultDto<PolicyDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        PolicyEffect? effect = null,
        bool? isActive = null,
        string sortBy = "Priority",
        bool sortDescending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las políticas activas del sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas activas</returns>
    Task<List<PolicyDto>> GetActivePoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las políticas activas que aplican a una acción específica.
    /// </summary>
    /// <param name="actionId">ID de la acción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de políticas activas para la acción</returns>
    Task<List<PolicyDto>> GetPoliciesForActionAsync(Guid actionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea una nueva política en el sistema.
    /// </summary>
    /// <param name="createDto">DTO con los datos de la política a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO de la política creada</returns>
    Task<PolicyDto> CreateAsync(CreatePolicyDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una política existente (solo información básica, no condiciones ni acciones).
    /// </summary>
    /// <param name="policyId">ID de la política a actualizar</param>
    /// <param name="updateDto">DTO con los datos actualizados</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO de la política actualizada</returns>
    Task<PolicyDto> UpdateAsync(Guid policyId, UpdatePolicyDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una política del sistema (soft delete).
    /// </summary>
    /// <param name="policyId">ID de la política a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la eliminación fue exitosa</returns>
    Task<bool> DeleteAsync(Guid policyId, CancellationToken cancellationToken = default);

    #endregion

    #region Policy Activation

    /// <summary>
    /// Activa una política para que sea evaluada por el motor ABAC.
    /// </summary>
    /// <param name="policyId">ID de la política a activar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO de la política activada</returns>
    Task<PolicyDto> ActivateAsync(Guid policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desactiva una política para que no sea evaluada por el motor ABAC.
    /// </summary>
    /// <param name="policyId">ID de la política a desactivar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO de la política desactivada</returns>
    Task<PolicyDto> DeactivateAsync(Guid policyId, CancellationToken cancellationToken = default);

    #endregion

    #region Condition Management

    /// <summary>
    /// Obtiene todas las condiciones de una política.
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de condiciones de la política</returns>
    Task<List<PolicyConditionDto>> GetConditionsAsync(Guid policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade una nueva condición a una política.
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="createDto">DTO con los datos de la condición a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO de la condición creada</returns>
    Task<PolicyConditionDto> AddConditionAsync(Guid policyId, CreatePolicyConditionDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una condición existente de una política.
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="conditionId">ID de la condición a actualizar</param>
    /// <param name="updateDto">DTO con los datos actualizados</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>DTO de la condición actualizada</returns>
    Task<PolicyConditionDto> UpdateConditionAsync(Guid policyId, Guid conditionId, UpdatePolicyConditionDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una condición de una política.
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="conditionId">ID de la condición a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la eliminación fue exitosa</returns>
    Task<bool> RemoveConditionAsync(Guid policyId, Guid conditionId, CancellationToken cancellationToken = default);

    #endregion

    #region Action Association Management

    /// <summary>
    /// Obtiene todas las acciones asociadas a una política.
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de acciones asociadas</returns>
    Task<List<ActionDto>> GetActionsAsync(Guid policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asocia una o más acciones a una política.
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="actionIds">IDs de las acciones a asociar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la asociación fue exitosa</returns>
    Task<bool> AssociateActionsAsync(Guid policyId, List<Guid> actionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desasocia una acción de una política.
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="actionId">ID de la acción a desasociar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la desasociación fue exitosa</returns>
    Task<bool> DisassociateActionAsync(Guid policyId, Guid actionId, CancellationToken cancellationToken = default);

    #endregion

    #region Validation and Statistics

    /// <summary>
    /// Valida si una política tiene una configuración correcta (al menos una condición y una acción).
    /// </summary>
    /// <param name="policyId">ID de la política a validar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la política es válida para ser activada</returns>
    Task<bool> ValidatePolicyAsync(Guid policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de una política (número de condiciones, acciones, prioridad, etc.).
    /// </summary>
    /// <param name="policyId">ID de la política</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Objeto con estadísticas de la política</returns>
    Task<PolicyStatisticsDto> GetStatisticsAsync(Guid policyId, CancellationToken cancellationToken = default);

    #endregion
}
