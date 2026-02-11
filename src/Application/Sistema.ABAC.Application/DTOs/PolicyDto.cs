using Sistema.ABAC.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para políticas ABAC del sistema.
/// </summary>
public class PolicyDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre de la política es requerido")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public PolicyEffect Effect { get; set; }

    [Range(0, 999, ErrorMessage = "La prioridad debe estar entre 0 y 999")]
    public int Priority { get; set; }

    public bool IsActive { get; set; }

    public List<PolicyConditionDto> Conditions { get; set; } = new();
    public List<Guid> ActionIds { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear una nueva política.
/// </summary>
public class CreatePolicyDto
{
    [Required(ErrorMessage = "El nombre de la política es requerido")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public PolicyEffect Effect { get; set; }

    [Range(0, 999, ErrorMessage = "La prioridad debe estar entre 0 y 999")]
    public int Priority { get; set; } = 100;

    public bool IsActive { get; set; } = true;

    public List<CreatePolicyConditionDto> Conditions { get; set; } = new();
    public List<Guid> ActionIds { get; set; } = new();
}

/// <summary>
/// DTO para actualizar una política existente.
/// </summary>
public class UpdatePolicyDto
{
    [Required(ErrorMessage = "El nombre de la política es requerido")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public PolicyEffect Effect { get; set; }

    [Range(0, 999, ErrorMessage = "La prioridad debe estar entre 0 y 999")]
    public int Priority { get; set; }

    public bool IsActive { get; set; }
}
