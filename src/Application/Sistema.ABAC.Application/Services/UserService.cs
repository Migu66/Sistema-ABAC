using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Domain.Interfaces;
using UserEntity = Sistema.ABAC.Domain.Entities.User;
using UserAttributeEntity = Sistema.ABAC.Domain.Entities.UserAttribute;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Implementación del servicio de gestión de usuarios.
/// Proporciona operaciones CRUD y gestión de atributos para usuarios del sistema ABAC.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUnitOfWork unitOfWork,
        UserManager<UserEntity> userManager,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    #region CRUD Operations

    /// <inheritdoc/>
    public async Task<UserDto?> GetByIdAsync(Guid userId, bool includeAttributes = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo usuario con ID: {UserId}, incluirAtributos: {IncludeAttributes}", userId, includeAttributes);

        var user = includeAttributes
            ? await _unitOfWork.Users.GetWithAttributesAsync(userId, cancellationToken)
            : await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Usuario con ID {UserId} no encontrado", userId);
            return null;
        }

        var userDto = _mapper.Map<UserDto>(user);

        // Cargar roles del usuario
        var roles = await _userManager.GetRolesAsync(user);
        userDto.Roles = roles.ToList();

        return userDto;
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<UserDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? department = null,
        bool? isActive = null,
        string sortBy = "UserName",
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Obteniendo lista de usuarios - Página: {Page}, Tamaño: {PageSize}, Búsqueda: {SearchTerm}, Departamento: {Department}",
            page, pageSize, searchTerm, department);

        // Validar parámetros de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Crear query base
        var query = _userManager.Users.AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(search) ||
                u.Email!.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsDeleted != isActive.Value);
        }

        // Filtrar por departamento si se proporciona
        // Nota: El departamento se puede obtener de atributos en versiones futuras
        // Por ahora solo aplicamos los filtros básicos

        // Contar total antes de paginar
        var totalCount = query.Count();

        // Aplicar ordenamiento
        query = sortBy.ToLower() switch
        {
            "email" => sortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "fullname" => sortDescending
                ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            "createdat" => sortDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => sortDescending // UserName por defecto
                ? query.OrderByDescending(u => u.UserName)
                : query.OrderBy(u => u.UserName)
        };

        // Aplicar paginación
        var users = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Mapear a DTOs
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var userDto = _mapper.Map<UserDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();
            userDtos.Add(userDto);
        }

        return new PagedResultDto<UserDto>
        {
            Items = userDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<UserDto> UpdateAsync(Guid userId, UpdateUserDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando usuario {UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("Usuario", userId);
        }

        // Actualizar propiedades si se proporcionan
        if (!string.IsNullOrWhiteSpace(updateDto.FullName))
        {
            var nameParts = updateDto.FullName.Split(' ', 2);
            user.FirstName = nameParts[0];
            user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != user.Email)
        {
            var emailExists = await _userManager.FindByEmailAsync(updateDto.Email);
            if (emailExists != null && emailExists.Id != userId)
            {
                throw new ValidationException("Email", "El correo electrónico ya está en uso por otro usuario");
            }
            user.Email = updateDto.Email;
            user.EmailConfirmed = false; // Requerir confirmación del nuevo email
        }

        if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
        {
            user.PhoneNumber = updateDto.PhoneNumber;
        }

        if (updateDto.IsActive.HasValue)
        {
            user.IsDeleted = !updateDto.IsActive.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException("User", $"Error al actualizar usuario: {errors}");
        }

        _logger.LogInformation("Usuario {UserId} actualizado correctamente", userId);

        var userDto = _mapper.Map<UserDto>(user);
        var roles = await _userManager.GetRolesAsync(user);
        userDto.Roles = roles.ToList();

        return userDto;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando usuario {UserId} (soft delete)", userId);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("Usuario", userId);
        }

        // Soft delete
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException("User", $"Error al eliminar usuario: {errors}");
        }

        _logger.LogInformation("Usuario {UserId} eliminado correctamente", userId);
    }

    #endregion

    #region User Attributes Management

    /// <inheritdoc/>
    public async Task<IEnumerable<UserAttributeDto>> GetUserAttributesAsync(
        Guid userId,
        bool includeExpired = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo atributos del usuario {UserId}, incluirExpirados: {IncludeExpired}", userId, includeExpired);

        // Verificar que el usuario existe
        var userExists = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (userExists == null)
        {
            throw new NotFoundException("Usuario", userId);
        }

        IEnumerable<UserAttributeEntity> attributes;

        if (includeExpired)
        {
            // Obtener todos los atributos
            var user = await _unitOfWork.Users.GetWithAttributesAsync(userId, cancellationToken);
            attributes = user?.UserAttributes.AsEnumerable() ?? Enumerable.Empty<UserAttributeEntity>();
        }
        else
        {
            // Obtener solo atributos activos
            attributes = await _unitOfWork.Users.GetActiveAttributesAsync(userId, null, cancellationToken);
        }

        return _mapper.Map<IEnumerable<UserAttributeDto>>(attributes);
    }

    /// <inheritdoc/>
    public async Task<UserAttributeDto> AssignAttributeAsync(
        Guid userId,
        AssignUserAttributeDto assignDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Asignando atributo {AttributeId} al usuario {UserId}", assignDto.AttributeId, userId);

        // Verificar que el usuario existe
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Usuario", userId);
        }

        // Verificar que el atributo existe
        var attribute = await _unitOfWork.Attributes.GetByIdAsync(assignDto.AttributeId, cancellationToken);
        if (attribute == null)
        {
            throw new NotFoundException("Atributo", assignDto.AttributeId);
        }

        // Verificar si ya existe esta asignación
        var existingUserAttribute = await _unitOfWork.Users.GetWithAttributesAsync(userId, cancellationToken);
        var existing = existingUserAttribute?.UserAttributes
            .FirstOrDefault(ua => ua.AttributeId == assignDto.AttributeId && !ua.IsDeleted);

        if (existing != null)
        {
            throw new ValidationException("AttributeId", $"El usuario ya tiene asignado el atributo '{attribute.Name}'");
        }

        // Crear la nueva asignación
        var userAttribute = _mapper.Map<UserAttributeEntity>(assignDto);
        userAttribute.UserId = userId;
        userAttribute.AttributeId = assignDto.AttributeId;

        await _unitOfWork.UserAttributes.AddAsync(userAttribute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} asignado correctamente al usuario {UserId}", assignDto.AttributeId, userId);

        // Recargar con datos completos
        var result = await _unitOfWork.UserAttributes.GetByIdAsync(userAttribute.Id, cancellationToken);
        return _mapper.Map<UserAttributeDto>(result);
    }

    /// <inheritdoc/>
    public async Task<UserAttributeDto> UpdateAttributeAsync(
        Guid userId,
        Guid attributeId,
        UpdateUserAttributeDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando atributo {AttributeId} del usuario {UserId}", attributeId, userId);

        // Obtener usuario con atributos
        var user = await _unitOfWork.Users.GetWithAttributesAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Usuario", userId);
        }

        // Buscar el atributo específico
        var userAttribute = user.UserAttributes
            .FirstOrDefault(ua => ua.AttributeId == attributeId && !ua.IsDeleted);

        if (userAttribute == null)
        {
            throw new NotFoundException($"El usuario no tiene asignado el atributo con ID {attributeId}");
        }

        // Actualizar valores
        userAttribute.Value = updateDto.Value;
        userAttribute.ValidFrom = updateDto.ValidFrom;
        userAttribute.ValidTo = updateDto.ValidTo;
        userAttribute.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.UserAttributes.Update(userAttribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} del usuario {UserId} actualizado correctamente", attributeId, userId);

        return _mapper.Map<UserAttributeDto>(userAttribute);
    }

    /// <inheritdoc/>
    public async Task RemoveAttributeAsync(Guid userId, Guid attributeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removiendo atributo {AttributeId} del usuario {UserId}", attributeId, userId);

        // Obtener usuario con atributos
        var user = await _unitOfWork.Users.GetWithAttributesAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Usuario", userId);
        }

        // Buscar el atributo específico
        var userAttribute = user.UserAttributes
            .FirstOrDefault(ua => ua.AttributeId == attributeId && !ua.IsDeleted);

        if (userAttribute == null)
        {
            throw new NotFoundException($"El usuario no tiene asignado el atributo con ID {attributeId}");
        }

        // Soft delete
        _unitOfWork.UserAttributes.Remove(userAttribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Atributo {AttributeId} removido correctamente del usuario {UserId}", attributeId, userId);
    }

    #endregion

    #region Query Methods

    /// <inheritdoc/>
    public async Task<IEnumerable<UserDto>> GetUsersByAttributeAsync(
        string attributeKey,
        string attributeValue,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Buscando usuarios con atributo {AttributeKey} = {AttributeValue}", attributeKey, attributeValue);

        var users = await _unitOfWork.Users.GetByAttributeAsync(attributeKey, attributeValue, cancellationToken);

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var userDto = _mapper.Map<UserDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();
            userDtos.Add(userDto);
        }

        return userDtos;
    }

    #endregion
}

