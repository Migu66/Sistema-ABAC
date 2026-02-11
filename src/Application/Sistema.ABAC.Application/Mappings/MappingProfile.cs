using AutoMapper;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper que define los mapeos entre entidades del dominio y DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ============================================================
        // MAPEOS DE AUTENTICACIÓN
        // ============================================================

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Los roles se cargan manualmente
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore()) // No existe en la entidad
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted))
            .ForMember(dest => dest.Department, opt => opt.Ignore()); // Se obtiene de atributos

        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => 
                src.FullName.Split(' ', 2).FirstOrDefault() ?? ""))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => 
                src.FullName.Split(' ', 2).Skip(1).FirstOrDefault() ?? ""))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // ============================================================
        // MAPEOS DE ATRIBUTOS
        // ============================================================

        CreateMap<AttributeEntity, AttributeDto>();

        CreateMap<CreateAttributeDto, AttributeEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        CreateMap<UpdateAttributeDto, AttributeEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Key, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // ============================================================
        // MAPEOS DE RECURSOS
        // ============================================================

        CreateMap<Resource, ResourceDto>()
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.ResourceAttributes));

        CreateMap<CreateResourceDto, Resource>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        CreateMap<UpdateResourceDto, Resource>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // ============================================================
        // MAPEOS DE ATRIBUTOS DE RECURSOS
        // ============================================================

        CreateMap<ResourceAttribute, ResourceAttributeDto>()
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.Attribute.Name))
            .ForMember(dest => dest.AttributeKey, opt => opt.MapFrom(src => src.Attribute.Key));

        CreateMap<AssignResourceAttributeDto, ResourceAttribute>()
            .ForMember(dest => dest.ResourceId, opt => opt.Ignore())
            .ForMember(dest => dest.Resource, opt => opt.Ignore())
            .ForMember(dest => dest.Attribute, opt => opt.Ignore());

        // ============================================================
        // MAPEOS DE ACCIONES
        // ============================================================

        CreateMap<ActionEntity, ActionDto>();

        CreateMap<CreateActionDto, ActionEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        CreateMap<UpdateActionDto, ActionEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // ============================================================
        // MAPEOS DE POLÍTICAS
        // ============================================================

        CreateMap<Policy, PolicyDto>()
            .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
            .ForMember(dest => dest.ActionIds, opt => opt.MapFrom(src => 
                src.PolicyActions.Select(pa => pa.ActionId)));

        CreateMap<CreatePolicyDto, Policy>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Conditions, opt => opt.Ignore())
            .ForMember(dest => dest.PolicyActions, opt => opt.Ignore());

        CreateMap<UpdatePolicyDto, Policy>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Conditions, opt => opt.Ignore())
            .ForMember(dest => dest.PolicyActions, opt => opt.Ignore());

        // ============================================================
        // MAPEOS DE CONDICIONES DE POLÍTICAS
        // ============================================================

        CreateMap<PolicyCondition, PolicyConditionDto>();

        CreateMap<CreatePolicyConditionDto, PolicyCondition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PolicyId, opt => opt.Ignore())
            .ForMember(dest => dest.Policy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        CreateMap<UpdatePolicyConditionDto, PolicyCondition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PolicyId, opt => opt.Ignore())
            .ForMember(dest => dest.Policy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // ============================================================
        // MAPEOS DE ATRIBUTOS DE USUARIOS
        // ============================================================

        CreateMap<UserAttribute, UserAttributeDto>()
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.Attribute.Name))
            .ForMember(dest => dest.AttributeKey, opt => opt.MapFrom(src => src.Attribute.Key));

        CreateMap<AssignUserAttributeDto, UserAttribute>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Attribute, opt => opt.Ignore());

        CreateMap<UpdateUserAttributeDto, UserAttribute>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.AttributeId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Attribute, opt => opt.Ignore());

        // ============================================================
        // MAPEOS DE LOGS DE ACCESO
        // ============================================================

        CreateMap<AccessLog, AccessLogDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? src.User.UserName : null))
            .ForMember(dest => dest.ResourceName, opt => opt.MapFrom(src => 
                src.Resource != null ? src.Resource.Name : null))
            .ForMember(dest => dest.ActionName, opt => opt.MapFrom(src => 
                src.Action != null ? src.Action.Name : null))
            .ForMember(dest => dest.PolicyName, opt => opt.Ignore()); // Se carga manualmente si es necesario

        // ============================================================
        // MAPEOS DE ESTADÍSTICAS
        // ============================================================

        CreateMap<AccessLogStatistics, AccessLogStatisticsDto>();
    }
}
