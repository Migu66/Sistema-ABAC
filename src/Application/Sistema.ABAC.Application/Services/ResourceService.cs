using AutoMapper;
using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Implementación del servicio de gestión de recursos.
/// Proporciona operaciones CRUD para recursos y gestión de sus atributos ABAC.
/// </summary>
public class ResourceService : IResourceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ResourceService> _logger;

    public ResourceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ResourceService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region CRUD Operations

    /// <inheritdoc/>
    public async Task<ResourceDto?> GetByIdAsync(Guid resourceId, bool includeAttributes = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo recurso con ID: {ResourceId}, incluir atributos: {IncludeAttributes}", 
            resourceId, includeAttributes);

        Resource? resource;

        if (includeAttributes)
        {
            resource = await _unitOfWork.Resources.GetWithAttributesAsync(resourceId, cancellationToken);
        }
        else
        {
            resource = await _unitOfWork.Resources.GetByIdAsync(resourceId, cancellationToken);
        }

        if (resource == null)
        {
            _logger.LogWarning("Recurso con ID {ResourceId} no encontrado", resourceId);
            return null;
        }

        return _mapper.Map<ResourceDto>(resource);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ResourceDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? type = null,
        string sortBy = "Name",
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo lista de recursos - Página: {Page}, Tamaño: {PageSize}, Búsqueda: {SearchTerm}, Tipo: {Type}",
            page, pageSize, searchTerm, type);

        // Validar parámetros de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Crear query base
        var query = (await _unitOfWork.Resources.GetAllAsync(cancellationToken)).AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(search) ||
                r.Type.ToLower().Contains(search) ||
                (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        // Filtrar por tipo si se proporciona
        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        // Contar total antes de paginar
        var totalCount = query.Count();

        // Aplicar ordenamiento
        query = sortBy.ToLower() switch
        {
            "type" => sortDescending
                ? query.OrderByDescending(r => r.Type)
                : query.OrderBy(r => r.Type),
            "createdat" => sortDescending
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt),
            _ => sortDescending // Name por defecto
                ? query.OrderByDescending(r => r.Name)
                : query.OrderBy(r => r.Name)
        };

        // Aplicar paginación
        var resources = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Mapear a DTOs
        var resourceDtos = _mapper.Map<List<ResourceDto>>(resources);

        return new PagedResultDto<ResourceDto>
        {
            Items = resourceDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<ResourceDto> CreateAsync(CreateResourceDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando nuevo recurso: {ResourceName} (Tipo: {ResourceType})", 
            createDto.Name, createDto.Type);

        // Crear la entidad
        var resource = _mapper.Map<Resource>(createDto);
        resource.CreatedAt = DateTime.UtcNow;
        resource.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Resources.AddAsync(resource, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recurso {ResourceId} creado correctamente: {ResourceName}", 
            resource.Id, resource.Name);

        return _mapper.Map<ResourceDto>(resource);
    }

    /// <inheritdoc/>
    public async Task<ResourceDto> UpdateAsync(Guid resourceId, UpdateResourceDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando recurso {ResourceId}", resourceId);

        var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId, cancellationToken);
        if (resource == null)
        {
            throw new NotFoundException("Recurso", resourceId);
        }

        // Actualizar propiedades
        if (!string.IsNullOrWhiteSpace(updateDto.Name))
        {
            resource.Name = updateDto.Name;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Type))
        {
            resource.Type = updateDto.Type;
        }

        if (updateDto.Description != null) // Permitir vaciar la descripción
        {
            resource.Description = string.IsNullOrWhiteSpace(updateDto.Description) ? null : updateDto.Description;
        }

        resource.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Resources.Update(resource);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recurso {ResourceId} actualizado correctamente", resourceId);

        return _mapper.Map<ResourceDto>(resource);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando recurso {ResourceId} (soft delete)", resourceId);

        var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId, cancellationToken);
        if (resource == null)
        {
            throw new NotFoundException("Recurso", resourceId);
        }

        // Verificar si el recurso tiene atributos asignados
        var resourceWithAttributes = await _unitOfWork.Resources.GetWithAttributesAsync(resourceId, cancellationToken);
        if (resourceWithAttributes?.ResourceAttributes?.Any() == true)
        {
            _logger.LogWarning("Recurso {ResourceId} tiene atributos asignados. Se eliminarán en cascada.", resourceId);
            // En un escenario real, podrías querer eliminar los atributos primero o lanzar una excepción
            // Por ahora, continuamos con el soft delete y EF Core manejará la cascada
        }

        // Soft delete
        resource.IsDeleted = true;
        resource.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Resources.Update(resource);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recurso {ResourceId} eliminado correctamente", resourceId);

        return true;
    }

    #endregion

    #region Resource Attributes Management

    /// <inheritdoc/>
    public async Task<List<ResourceAttributeDto>> GetAttributesAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo atributos del recurso {ResourceId}", resourceId);

        var resource = await _unitOfWork.Resources.GetWithAttributesAsync(resourceId, cancellationToken);
        if (resource == null)
        {
            throw new NotFoundException("Recurso", resourceId);
        }

        var attributeDtos = _mapper.Map<List<ResourceAttributeDto>>(resource.ResourceAttributes);

        _logger.LogInformation("Se encontraron {Count} atributos para el recurso {ResourceId}", 
            attributeDtos.Count, resourceId);

        return attributeDtos;
    }

    /// <inheritdoc/>
    public async Task<ResourceAttributeDto> AssignAttributeAsync(
        Guid resourceId, 
        AssignResourceAttributeDto assignDto, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Asignando atributo {AttributeId} al recurso {ResourceId}", 
            assignDto.AttributeId, resourceId);

        // Verificar que el recurso existe
        var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId, cancellationToken);
        if (resource == null)
        {
            throw new NotFoundException("Recurso", resourceId);
        }

        // Verificar que el atributo existe
        var attribute = await _unitOfWork.Attributes.GetByIdAsync(assignDto.AttributeId, cancellationToken);
        if (attribute == null)
        {
            throw new NotFoundException("Atributo", assignDto.AttributeId);
        }

        // Verificar si ya existe esta asignación
        var existingAttributes = (await _unitOfWork.ResourceAttributes.GetAllAsync(cancellationToken))
            .Where(ra => ra.ResourceId == resourceId && ra.AttributeId == assignDto.AttributeId && !ra.IsDeleted)
            .ToList();

        if (existingAttributes.Any())
        {
            throw new ValidationException("ResourceAttribute", 
                $"El atributo '{attribute.Name}' ya está asignado a este recurso.");
        }

        // Crear la asignación
        var resourceAttribute = new ResourceAttribute
        {
            ResourceId = resourceId,
            AttributeId = assignDto.AttributeId,
            Value = assignDto.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ResourceAttributes.AddAsync(resourceAttribute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} asignado al recurso {ResourceId} correctamente", 
            assignDto.AttributeId, resourceId);

        // Recargar con navegación para mapear correctamente
        var createdAttribute = (await _unitOfWork.ResourceAttributes.GetAllAsync(cancellationToken))
            .FirstOrDefault(ra => ra.Id == resourceAttribute.Id);

        return _mapper.Map<ResourceAttributeDto>(createdAttribute);
    }

    /// <inheritdoc/>
    public async Task<ResourceAttributeDto> UpdateAttributeAsync(
        Guid resourceId, 
        Guid attributeId, 
        string newValue, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando atributo {AttributeId} del recurso {ResourceId}", 
            attributeId, resourceId);

        // Buscar la asignación existente
        var resourceAttribute = (await _unitOfWork.ResourceAttributes.GetAllAsync(cancellationToken))
            .FirstOrDefault(ra => ra.ResourceId == resourceId && ra.AttributeId == attributeId && !ra.IsDeleted);

        if (resourceAttribute == null)
        {
            throw new NotFoundException("ResourceAttribute", 
                $"No se encontró el atributo {attributeId} asignado al recurso {resourceId}");
        }

        // Actualizar el valor
        resourceAttribute.Value = newValue;
        resourceAttribute.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ResourceAttributes.Update(resourceAttribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} del recurso {ResourceId} actualizado correctamente", 
            attributeId, resourceId);

        return _mapper.Map<ResourceAttributeDto>(resourceAttribute);
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAttributeAsync(Guid resourceId, Guid attributeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removiendo atributo {AttributeId} del recurso {ResourceId}", 
            attributeId, resourceId);

        // Buscar la asignación existente
        var resourceAttribute = (await _unitOfWork.ResourceAttributes.GetAllAsync(cancellationToken))
            .FirstOrDefault(ra => ra.ResourceId == resourceId && ra.AttributeId == attributeId && !ra.IsDeleted);

        if (resourceAttribute == null)
        {
            throw new NotFoundException("ResourceAttribute", 
                $"No se encontró el atributo {attributeId} asignado al recurso {resourceId}");
        }

        // Soft delete
        resourceAttribute.IsDeleted = true;
        resourceAttribute.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ResourceAttributes.Update(resourceAttribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} removido del recurso {ResourceId} correctamente", 
            attributeId, resourceId);

        return true;
    }

    #endregion
}
