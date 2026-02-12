using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para acciones que se pueden realizar sobre recursos.
/// </summary>
public class ActionDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre de la acción es requerido")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código de la acción es requerido")]
    [MaxLength(50)]
    [RegularExpression(@"^[a-z_][a-z0-9_]*$", 
        ErrorMessage = "El código debe ser en minúsculas, usar snake_case y comenzar con letra")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear una nueva acción.
/// </summary>
public class CreateActionDto
{
    [Required(ErrorMessage = "El nombre de la acción es requerido")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código de la acción es requerido")]
    [MaxLength(50)]
    [RegularExpression(@"^[a-z_][a-z0-9_]*$", 
        ErrorMessage = "El código debe ser en minúsculas, usar snake_case y comenzar con letra")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// DTO para actualizar una acción existente.
/// </summary>
public class UpdateActionDto
{
    [Required(ErrorMessage = "El nombre de la acción es requerido")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
