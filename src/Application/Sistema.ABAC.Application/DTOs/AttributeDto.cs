using Sistema.ABAC.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para definiciones de atributos del sistema ABAC.
/// </summary>
public class AttributeDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre del atributo es requerido")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave del atributo es requerida")]
    [MaxLength(100)]
    [RegularExpression(@"^[a-z_][a-z0-9_]*$", 
        ErrorMessage = "La clave debe ser en minúsculas, usar snake_case y comenzar con letra")]
    public string Key { get; set; } = string.Empty;

    [Required]
    public AttributeType Type { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear un nuevo atributo.
/// </summary>
public class CreateAttributeDto
{
    [Required(ErrorMessage = "El nombre del atributo es requerido")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave del atributo es requerida")]
    [MaxLength(100)]
    [RegularExpression(@"^[a-z_][a-z0-9_]*$", 
        ErrorMessage = "La clave debe ser en minúsculas, usar snake_case y comenzar con letra")]
    public string Key { get; set; } = string.Empty;

    [Required]
    public AttributeType Type { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// DTO para actualizar un atributo existente.
/// </summary>
public class UpdateAttributeDto
{
    [Required(ErrorMessage = "El nombre del atributo es requerido")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
