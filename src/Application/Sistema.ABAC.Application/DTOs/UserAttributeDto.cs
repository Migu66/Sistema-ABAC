using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para atributos asignados a usuarios.
/// </summary>
public class UserAttributeDto
{
    public Guid UserId { get; set; }
    public Guid AttributeId { get; set; }

    public string AttributeName { get; set; } = string.Empty;
    public string AttributeKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "El valor del atributo es requerido")]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

/// <summary>
/// DTO para asignar un atributo a un usuario.
/// </summary>
public class AssignUserAttributeDto
{
    [Required(ErrorMessage = "El ID del atributo es requerido")]
    public Guid AttributeId { get; set; }

    [Required(ErrorMessage = "El valor del atributo es requerido")]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

/// <summary>
/// DTO para actualizar un atributo de usuario.
/// </summary>
public class UpdateUserAttributeDto
{
    [Required(ErrorMessage = "El valor del atributo es requerido")]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
