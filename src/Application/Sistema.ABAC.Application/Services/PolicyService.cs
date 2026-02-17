using AutoMapper;
using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Domain.Interfaces;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Implementación del servicio de gestión de políticas ABAC.
/// Proporciona operaciones completas para políticas, condiciones y acciones asociadas.
/// </summary>
public class PolicyService : IPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PolicyService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region CRUD Operations

    /// <inheritdoc/>
    public async Task<PolicyDto?> GetByIdAsync(Guid policyId, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo política con ID: {PolicyId}, incluir detalles: {IncludeDetails}", policyId, includeDetails);

        Policy? policy;

        if (includeDetails)
        {
            policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        }
        else
        {
            policy = await _unitOfWork.Policies.GetByIdAsync(policyId, cancellationToken);
        }

        if (policy == null)
        {
            _logger.LogWarning("Política con ID {PolicyId} no encontrada", policyId);
            return null;
        }

        return _mapper.Map<PolicyDto>(policy);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<PolicyDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        PolicyEffect? effect = null,
        bool? isActive = null,
        string sortBy = "Priority",
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo lista de políticas - Página: {Page}, Tamaño: {PageSize}, Búsqueda: {SearchTerm}, Efecto: {Effect}, Activa: {IsActive}",
            page, pageSize, searchTerm, effect, isActive);

        // Validar parámetros de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Crear query base
        var query = (await _unitOfWork.Policies.GetAllAsync(cancellationToken)).AsQueryable();

        // Aplicar filtros de búsqueda
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        // Filtrar por efecto
        if (effect.HasValue)
        {
            query = query.Where(p => p.Effect == effect.Value);
        }

        // Filtrar por estado activo/inactivo
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // Contar total antes de paginar
        var totalCount = query.Count();

        // Aplicar ordenamiento
        query = sortBy.ToLower() switch
        {
            "name" => sortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "effect" => sortDescending
                ? query.OrderByDescending(p => p.Effect)
                : query.OrderBy(p => p.Effect),
            "createdat" => sortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => sortDescending // Priority por defecto
                ? query.OrderByDescending(p => p.Priority)
                : query.OrderBy(p => p.Priority)
        };

        // Aplicar paginación
        var policies = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Mapear a DTOs
        var policyDtos = _mapper.Map<List<PolicyDto>>(policies);

        return new PagedResultDto<PolicyDto>
        {
            Items = policyDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<List<PolicyDto>> GetActivePoliciesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo todas las políticas activas");

        var policies = await _unitOfWork.Policies.GetActivePoliciesAsync(cancellationToken);
        
        return _mapper.Map<List<PolicyDto>>(policies);
    }

    /// <inheritdoc/>
    public async Task<List<PolicyDto>> GetPoliciesForActionAsync(Guid actionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo políticas activas para acción: {ActionId}", actionId);

        var policies = await _unitOfWork.Policies.GetActivePoliciesForActionAsync(actionId, cancellationToken);
        
        return _mapper.Map<List<PolicyDto>>(policies);
    }

    /// <inheritdoc/>
    public async Task<PolicyDto> CreateAsync(CreatePolicyDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando nueva política: {PolicyName}", createDto.Name);

        // Crear la entidad Policy
        var policy = _mapper.Map<Policy>(createDto);
        policy.CreatedAt = DateTime.UtcNow;
        policy.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Policies.AddAsync(policy, cancellationToken);

        // Agregar condiciones si se proporcionaron
        if (createDto.Conditions?.Any() == true)
        {
            foreach (var conditionDto in createDto.Conditions)
            {
                var condition = _mapper.Map<PolicyCondition>(conditionDto);
                condition.PolicyId = policy.Id;
                condition.CreatedAt = DateTime.UtcNow;
                condition.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.PolicyConditions.AddAsync(condition, cancellationToken);
            }
        }

        // Asociar acciones si se proporcionaron
        if (createDto.ActionIds?.Any() == true)
        {
            // Validar que todas las acciones existan
            foreach (var actionId in createDto.ActionIds)
            {
                var actionExists = await _unitOfWork.Actions.GetByIdAsync(actionId, cancellationToken);
                if (actionExists == null)
                {
                    throw new NotFoundException("Acción", actionId);
                }

                var policyAction = new PolicyAction
                {
                    PolicyId = policy.Id,
                    ActionId = actionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.PolicyActions.AddAsync(policyAction, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Política {PolicyId} creada correctamente: {PolicyName}", policy.Id, policy.Name);

        // Retornar con detalles completos
        return (await GetByIdAsync(policy.Id, true, cancellationToken))!;
    }

    /// <inheritdoc/>
    public async Task<PolicyDto> UpdateAsync(Guid policyId, UpdatePolicyDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando política {PolicyId}", policyId);

        var policy = await _unitOfWork.Policies.GetByIdAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        // Actualizar propiedades básicas
        policy.Name = updateDto.Name;
        policy.Description = updateDto.Description;
        policy.Effect = updateDto.Effect;
        policy.Priority = updateDto.Priority;
        policy.IsActive = updateDto.IsActive;
        policy.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Policies.Update(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Política {PolicyId} actualizada correctamente", policyId);

        return _mapper.Map<PolicyDto>(policy);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando política {PolicyId} (soft delete)", policyId);

        var policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        // Soft delete de la política
        policy.IsDeleted = true;
        policy.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Policies.Update(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Política {PolicyId} eliminada correctamente", policyId);

        return true;
    }

    #endregion

    #region Policy Activation

    /// <inheritdoc/>
    public async Task<PolicyDto> ActivateAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activando política {PolicyId}", policyId);

        var policy = await _unitOfWork.Policies.GetByIdAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        if (policy.IsActive)
        {
            _logger.LogWarning("La política {PolicyId} ya está activa", policyId);
            return _mapper.Map<PolicyDto>(policy);
        }

        // Validar que la política tenga configuración mínima antes de activar
        var isValid = await ValidatePolicyAsync(policyId, cancellationToken);
        if (!isValid)
        {
            throw new ValidationException("Policy", 
                "La política no puede ser activada. Debe tener al menos una condición y una acción asociada.");
        }

        policy.IsActive = true;
        policy.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Policies.Update(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Política {PolicyId} activada correctamente", policyId);

        return _mapper.Map<PolicyDto>(policy);
    }

    /// <inheritdoc/>
    public async Task<PolicyDto> DeactivateAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Desactivando política {PolicyId}", policyId);

        var policy = await _unitOfWork.Policies.GetByIdAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        if (!policy.IsActive)
        {
            _logger.LogWarning("La política {PolicyId} ya está inactiva", policyId);
            return _mapper.Map<PolicyDto>(policy);
        }

        policy.IsActive = false;
        policy.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Policies.Update(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Política {PolicyId} desactivada correctamente", policyId);

        return _mapper.Map<PolicyDto>(policy);
    }

    #endregion

    #region Condition Management

    /// <inheritdoc/>
    public async Task<List<PolicyConditionDto>> GetConditionsAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo condiciones de política {PolicyId}", policyId);

        var policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        return _mapper.Map<List<PolicyConditionDto>>(policy.Conditions);
    }

    /// <inheritdoc/>
    public async Task<PolicyConditionDto> AddConditionAsync(Guid policyId, CreatePolicyConditionDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Añadiendo condición a política {PolicyId}", policyId);

        var policy = await _unitOfWork.Policies.GetByIdAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        var condition = _mapper.Map<PolicyCondition>(createDto);
        condition.PolicyId = policyId;
        condition.CreatedAt = DateTime.UtcNow;
        condition.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PolicyConditions.AddAsync(condition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Condición {ConditionId} añadida a política {PolicyId}", condition.Id, policyId);

        return _mapper.Map<PolicyConditionDto>(condition);
    }

    /// <inheritdoc/>
    public async Task<PolicyConditionDto> UpdateConditionAsync(Guid policyId, Guid conditionId, UpdatePolicyConditionDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando condición {ConditionId} de política {PolicyId}", conditionId, policyId);

        var policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        var condition = policy.Conditions?.FirstOrDefault(c => c.Id == conditionId);
        if (condition == null)
        {
            throw new NotFoundException($"La condición {conditionId} no pertenece a la política {policyId}");
        }

        // Actualizar propiedades
        condition.AttributeType = updateDto.AttributeType;
        condition.AttributeKey = updateDto.AttributeKey;
        condition.Operator = updateDto.Operator;
        condition.ExpectedValue = updateDto.ExpectedValue;
        condition.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.PolicyConditions.Update(condition);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Condición {ConditionId} actualizada correctamente", conditionId);

        return _mapper.Map<PolicyConditionDto>(condition);
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveConditionAsync(Guid policyId, Guid conditionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando condición {ConditionId} de política {PolicyId}", conditionId, policyId);

        var policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        var condition = policy.Conditions?.FirstOrDefault(c => c.Id == conditionId);
        if (condition == null)
        {
            throw new NotFoundException($"La condición {conditionId} no pertenece a la política {policyId}");
        }

        // Verificar que no sea la única condición si la política está activa
        if (policy.IsActive && policy.Conditions?.Count <= 1)
        {
            throw new ValidationException("Condition", 
                "No se puede eliminar la única condición de una política activa. Desactive la política primero.");
        }

        _unitOfWork.PolicyConditions.Remove(condition);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Condición {ConditionId} eliminada correctamente", conditionId);

        return true;
    }

    #endregion

    #region Action Association Management

    /// <inheritdoc/>
    public async Task<List<ActionDto>> GetActionsAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo acciones de política {PolicyId}", policyId);

        var policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        var actions = policy.PolicyActions?.Select(pa => pa.Action).Where(a => a != null).ToList() ?? new List<Domain.Entities.Action>();
        
        return _mapper.Map<List<ActionDto>>(actions);
    }

    /// <inheritdoc/>
    public async Task<bool> AssociateActionsAsync(Guid policyId, List<Guid> actionIds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Asociando {Count} acciones a política {PolicyId}", actionIds.Count, policyId);

        var policy = await _unitOfWork.Policies.GetByIdAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        // Validar que todas las acciones existan
        foreach (var actionId in actionIds)
        {
            var actionExists = await _unitOfWork.Actions.GetByIdAsync(actionId, cancellationToken);
            if (actionExists == null)
            {
                throw new NotFoundException("Acción", actionId);
            }

            // Verificar si ya está asociada
            var existingAssociation = (await _unitOfWork.PolicyActions.GetAllAsync(cancellationToken))
                .FirstOrDefault(pa => pa.PolicyId == policyId && pa.ActionId == actionId);

            if (existingAssociation == null)
            {
                var policyAction = new PolicyAction
                {
                    PolicyId = policyId,
                    ActionId = actionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.PolicyActions.AddAsync(policyAction, cancellationToken);
            }
            else
            {
                _logger.LogDebug("La acción {ActionId} ya está asociada a la política {PolicyId}", actionId, policyId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Acciones asociadas correctamente a política {PolicyId}", policyId);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DisassociateActionAsync(Guid policyId, Guid actionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Desasociando acción {ActionId} de política {PolicyId}", actionId, policyId);

        var policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        var policyAction = policy.PolicyActions?.FirstOrDefault(pa => pa.ActionId == actionId);
        if (policyAction == null)
        {
            throw new NotFoundException($"La acción {actionId} no está asociada a la política {policyId}");
        }

        // Verificar que no sea la única acción si la política está activa
        if (policy.IsActive && policy.PolicyActions?.Count <= 1)
        {
            throw new ValidationException("Action", 
                "No se puede desasociar la única acción de una política activa. Desactive la política primero.");
        }

        _unitOfWork.PolicyActions.Remove(policyAction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Acción {ActionId} desasociada de política {PolicyId}", actionId, policyId);

        return true;
    }

    #endregion

    #region Validation and Statistics

    /// <inheritdoc/>
    public async Task<bool> ValidatePolicyAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validando política {PolicyId}", policyId);

        var policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            return false;
        }

        // Una política válida debe tener al menos:
        // 1. Una condición
        // 2. Una acción asociada
        var hasConditions = policy.Conditions?.Any() == true;
        var hasActions = policy.PolicyActions?.Any() == true;

        var isValid = hasConditions && hasActions;

        _logger.LogDebug(
            "Validación de política {PolicyId}: Condiciones={HasConditions}, Acciones={HasActions}, Válida={IsValid}",
            policyId, hasConditions, hasActions, isValid);

        return isValid;
    }

    /// <inheritdoc/>
    public async Task<PolicyStatisticsDto> GetStatisticsAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo estadísticas de política {PolicyId}", policyId);

        var policy = await _unitOfWork.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException("Política", policyId);
        }

        // Obtener conteo de aplicaciones de la política desde logs
        var applicationCount = await _unitOfWork.Policies.GetPolicyApplicationCountAsync(policyId, null, null, cancellationToken);

        // Aquí podrías obtener la última fecha de evaluación desde los logs si está implementado
        // Por ahora lo dejamos como null

        var statistics = new PolicyStatisticsDto
        {
            PolicyId = policy.Id,
            Name = policy.Name,
            ConditionsCount = policy.Conditions?.Count ?? 0,
            ActionsCount = policy.PolicyActions?.Count ?? 0,
            IsActive = policy.IsActive,
            Priority = policy.Priority,
            Effect = policy.Effect,
            AccessLogsCount = applicationCount,
            LastEvaluated = null // TODO: Implementar cuando se tenga el sistema de logs completo
        };

        return statistics;
    }

    #endregion
}
