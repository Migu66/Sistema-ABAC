using AutoMapper;
using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Domain.Enums;
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Implementación del servicio de gestión de atributos.
/// Proporciona operaciones CRUD para definiciones de atributos del sistema ABAC.
/// </summary>
public class AttributeService : IAttributeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AttributeService> _logger;

    public AttributeService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AttributeService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region CRUD Operations

    /// <inheritdoc/>
    public async Task<AttributeDto?> GetByIdAsync(Guid attributeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo atributo con ID: {AttributeId}", attributeId);

        var attribute = await _unitOfWork.Attributes.GetByIdAsync(attributeId, cancellationToken);

        if (attribute == null)
        {
            _logger.LogWarning("Atributo con ID {AttributeId} no encontrado", attributeId);
            return null;
        }

        return _mapper.Map<AttributeDto>(attribute);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<AttributeDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? type = null,
        string sortBy = "Name",
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo lista de atributos - Página: {Page}, Tamaño: {PageSize}, Búsqueda: {SearchTerm}, Tipo: {Type}",
            page, pageSize, searchTerm, type);

        // Validar parámetros de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Crear query base
        var query = (await _unitOfWork.Attributes.GetAllAsync(cancellationToken)).AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(search) ||
                a.Key.ToLower().Contains(search) ||
                (a.Description != null && a.Description.ToLower().Contains(search)));
        }

        // Filtrar por tipo si se proporciona
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<AttributeType>(type, true, out var attributeType))
        {
            query = query.Where(a => a.Type == attributeType);
        }

        // Contar total antes de paginar
        var totalCount = query.Count();

        // Aplicar ordenamiento
        query = sortBy.ToLower() switch
        {
            "key" => sortDescending
                ? query.OrderByDescending(a => a.Key)
                : query.OrderBy(a => a.Key),
            "type" => sortDescending
                ? query.OrderByDescending(a => a.Type)
                : query.OrderBy(a => a.Type),
            "createdat" => sortDescending
                ? query.OrderByDescending(a => a.CreatedAt)
                : query.OrderBy(a => a.CreatedAt),
            _ => sortDescending // Name por defecto
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name)
        };

        // Aplicar paginación
        var attributes = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Mapear a DTOs
        var attributeDtos = _mapper.Map<List<AttributeDto>>(attributes);

        return new PagedResultDto<AttributeDto>
        {
            Items = attributeDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<AttributeDto> CreateAsync(CreateAttributeDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando nuevo atributo con clave: {Key}", createDto.Key);

        // Validar que la clave no exista
        var existingByKey = await _unitOfWork.Attributes.GetByKeyAsync(createDto.Key, cancellationToken);
        if (existingByKey != null)
        {
            throw new ValidationException("Key", $"Ya existe un atributo con la clave '{createDto.Key}'");
        }

        // Crear la entidad
        var attribute = _mapper.Map<AttributeEntity>(createDto);
        attribute.CreatedAt = DateTime.UtcNow;
        attribute.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Attributes.AddAsync(attribute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} creado correctamente con clave: {Key}", attribute.Id, attribute.Key);

        return _mapper.Map<AttributeDto>(attribute);
    }

    /// <inheritdoc/>
    public async Task<AttributeDto> UpdateAsync(Guid attributeId, UpdateAttributeDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando atributo {AttributeId}", attributeId);

        var attribute = await _unitOfWork.Attributes.GetByIdAsync(attributeId, cancellationToken);
        if (attribute == null)
        {
            throw new NotFoundException("Atributo", attributeId);
        }

        // Actualizar propiedades
        if (!string.IsNullOrWhiteSpace(updateDto.Name))
        {
            attribute.Name = updateDto.Name;
        }

        if (updateDto.Description != null) // Permitir vaciar la descripción
        {
            attribute.Description = string.IsNullOrWhiteSpace(updateDto.Description) ? null : updateDto.Description;
        }

        attribute.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Attributes.Update(attribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} actualizado correctamente", attributeId);

        return _mapper.Map<AttributeDto>(attribute);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid attributeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando atributo {AttributeId} (soft delete)", attributeId);

        var attribute = await _unitOfWork.Attributes.GetByIdAsync(attributeId, cancellationToken);
        if (attribute == null)
        {
            throw new NotFoundException("Atributo", attributeId);
        }

        // Verificar si el atributo está en uso
        // Esto podría expandirse para verificar si está en UserAttributes, ResourceAttributes o PolicyConditions
        var attributeWithUsers = await _unitOfWork.Attributes.GetWithUserAttributesAsync(attributeId, cancellationToken);
        if (attributeWithUsers?.UserAttributes?.Any() == true)
        {
            throw new ValidationException("Attribute", 
                "No se puede eliminar el atributo porque está asignado a usuarios. Primero debe desasignarlo.");
        }

        var attributeWithResources = await _unitOfWork.Attributes.GetWithResourceAttributesAsync(attributeId, cancellationToken);
        if (attributeWithResources?.ResourceAttributes?.Any() == true)
        {
            throw new ValidationException("Attribute", 
                "No se puede eliminar el atributo porque está asignado a recursos. Primero debe desasignarlo.");
        }

        // Soft delete
        attribute.IsDeleted = true;
        attribute.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Attributes.Update(attribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} eliminado correctamente", attributeId);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByKeyAsync(string key, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Verificando si existe atributo con clave: {Key}, excluir ID: {ExcludeId}", key, excludeId);

        return await _unitOfWork.Attributes.KeyExistsAsync(key, excludeId, cancellationToken);
    }

    #endregion
}
