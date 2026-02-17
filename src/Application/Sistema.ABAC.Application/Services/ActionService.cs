using AutoMapper;
using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Domain.Interfaces;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Implementación del servicio de gestión de acciones.
/// Proporciona operaciones CRUD para acciones que pueden realizarse sobre recursos del sistema ABAC.
/// </summary>
public class ActionService : IActionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ActionService> _logger;

    public ActionService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ActionService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region CRUD Operations

    /// <inheritdoc/>
    public async Task<ActionDto?> GetByIdAsync(Guid actionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo acción con ID: {ActionId}", actionId);

        var action = await _unitOfWork.Actions.GetByIdAsync(actionId, cancellationToken);

        if (action == null)
        {
            _logger.LogWarning("Acción con ID {ActionId} no encontrada", actionId);
            return null;
        }

        return _mapper.Map<ActionDto>(action);
    }

    /// <inheritdoc/>
    public async Task<ActionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo acción con código: {Code}", code);

        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Se intentó buscar acción con código nulo o vacío");
            return null;
        }

        var action = await _unitOfWork.Actions.GetByCodeAsync(code, cancellationToken);

        if (action == null)
        {
            _logger.LogWarning("Acción con código {Code} no encontrada", code);
            return null;
        }

        return _mapper.Map<ActionDto>(action);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ActionDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string sortBy = "Name",
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo lista de acciones - Página: {Page}, Tamaño: {PageSize}, Búsqueda: {SearchTerm}",
            page, pageSize, searchTerm);

        // Validar parámetros de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Crear query base
        var query = (await _unitOfWork.Actions.GetAllAsync(cancellationToken)).AsQueryable();

        // Aplicar filtros de búsqueda
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(search) ||
                a.Code.ToLower().Contains(search) ||
                (a.Description != null && a.Description.ToLower().Contains(search)));
        }

        // Contar total antes de paginar
        var totalCount = query.Count();

        // Aplicar ordenamiento
        query = sortBy.ToLower() switch
        {
            "code" => sortDescending
                ? query.OrderByDescending(a => a.Code)
                : query.OrderBy(a => a.Code),
            "createdat" => sortDescending
                ? query.OrderByDescending(a => a.CreatedAt)
                : query.OrderBy(a => a.CreatedAt),
            _ => sortDescending // Name por defecto
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name)
        };

        // Aplicar paginación
        var actions = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Mapear a DTOs
        var actionDtos = _mapper.Map<List<ActionDto>>(actions);

        return new PagedResultDto<ActionDto>
        {
            Items = actionDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<ActionDto> CreateAsync(CreateActionDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando nueva acción con código: {Code}", createDto.Code);

        // Validar que el código no exista
        var existingByCode = await _unitOfWork.Actions.GetByCodeAsync(createDto.Code, cancellationToken);
        if (existingByCode != null)
        {
            throw new ValidationException("Code", $"Ya existe una acción con el código '{createDto.Code}'");
        }

        // Crear la entidad
        var action = _mapper.Map<ActionEntity>(createDto);
        action.CreatedAt = DateTime.UtcNow;
        action.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Actions.AddAsync(action, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Acción {ActionId} creada correctamente con código: {Code}", action.Id, action.Code);

        return _mapper.Map<ActionDto>(action);
    }

    /// <inheritdoc/>
    public async Task<ActionDto> UpdateAsync(Guid actionId, UpdateActionDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando acción {ActionId}", actionId);

        var action = await _unitOfWork.Actions.GetByIdAsync(actionId, cancellationToken);
        if (action == null)
        {
            throw new NotFoundException("Acción", actionId);
        }

        // Actualizar propiedades (el código NO se puede cambiar una vez creado)
        if (!string.IsNullOrWhiteSpace(updateDto.Name))
        {
            action.Name = updateDto.Name;
        }

        if (updateDto.Description != null) // Permitir vaciar la descripción
        {
            action.Description = string.IsNullOrWhiteSpace(updateDto.Description) ? null : updateDto.Description;
        }

        action.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Actions.Update(action);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Acción {ActionId} actualizada correctamente", actionId);

        return _mapper.Map<ActionDto>(action);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid actionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando acción {ActionId} (soft delete)", actionId);

        var action = await _unitOfWork.Actions.GetByIdAsync(actionId, cancellationToken);
        if (action == null)
        {
            throw new NotFoundException("Acción", actionId);
        }

        // Verificar si la acción está en uso en políticas
        var actionWithPolicies = await _unitOfWork.Actions.GetWithPoliciesAsync(actionId, cancellationToken);
        if (actionWithPolicies?.PolicyActions?.Any() == true)
        {
            var activePoliciesCount = actionWithPolicies.PolicyActions.Count(pa => pa.Policy?.IsActive == true);
            if (activePoliciesCount > 0)
            {
                throw new ValidationException("Action",
                    $"No se puede eliminar la acción porque está asociada a {activePoliciesCount} política(s) activa(s). " +
                    "Primero debe desactivar o desasociar las políticas.");
            }

            _logger.LogWarning(
                "La acción {ActionId} tiene {PolicyCount} política(s) asociada(s) pero inactiva(s). Se procederá con la eliminación.",
                actionId, actionWithPolicies.PolicyActions.Count);
        }

        // Verificar si hay logs de auditoría que referencien esta acción (solo informativo)
        var actionLogs = await _unitOfWork.Actions.GetAccessLogsAsync(actionId, 0, 1, cancellationToken);
        if (actionLogs.Any())
        {
            _logger.LogInformation(
                "La acción {ActionId} tiene registros en el log de auditoría. Estos se mantendrán para trazabilidad.",
                actionId);
        }

        // Soft delete
        action.IsDeleted = true;
        action.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Actions.Update(action);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Acción {ActionId} eliminada correctamente", actionId);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeActionId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Verificando si existe acción con código: {Code}, excluir ID: {ExcludeActionId}", code, excludeActionId);

        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return await _unitOfWork.Actions.CodeExistsAsync(code, excludeActionId, cancellationToken);
    }

    #endregion
}
