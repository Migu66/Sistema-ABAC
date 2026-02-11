using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para recursos protegidos por el sistema ABAC.
/// </summary>
public class ResourceDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre del recurso es requerido")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo del recurso es requerido")]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<ResourceAttributeDto> Attributes { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear un nuevo recurso.
/// </summary>
public class CreateResourceDto
{
    [Required(ErrorMessage = "El nombre del recurso es requerido")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo del recurso es requerido")]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// DTO para actualizar un recurso existente.
/// </summary>
public class UpdateResourceDto
{
    [Required(ErrorMessage = "El nombre del recurso es requerido")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo del recurso es requerido")]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// DTO para asignar un atributo a un recurso.
/// </summary>
public class AssignResourceAttributeDto
{
    [Required(ErrorMessage = "El ID del atributo es requerido")]
    public Guid AttributeId { get; set; }

    [Required(ErrorMessage = "El valor del atributo es requerido")]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;
}
