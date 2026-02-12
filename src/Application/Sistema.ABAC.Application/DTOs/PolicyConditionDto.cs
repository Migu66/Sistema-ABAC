using Sistema.ABAC.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para condiciones de una política ABAC.
/// </summary>
public class PolicyConditionDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }

    [Required(ErrorMessage = "El tipo de atributo es requerido")]
    [MaxLength(50)]
    public string AttributeType { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave del atributo es requerida")]
    [MaxLength(100)]
    public string AttributeKey { get; set; } = string.Empty;

    [Required]
    public OperatorType Operator { get; set; }

    [Required(ErrorMessage = "El valor esperado es requerido")]
    [MaxLength(500)]
    public string ExpectedValue { get; set; } = string.Empty;
}

/// <summary>
/// DTO para crear una nueva condición de política.
/// </summary>
public class CreatePolicyConditionDto
{
    [Required(ErrorMessage = "El tipo de atributo es requerido")]
    [MaxLength(50)]
    public string AttributeType { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave del atributo es requerida")]
    [MaxLength(100)]
    public string AttributeKey { get; set; } = string.Empty;

    [Required]
    public OperatorType Operator { get; set; }

    [Required(ErrorMessage = "El valor esperado es requerido")]
    [MaxLength(500)]
    public string ExpectedValue { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualizar una condición de política.
/// </summary>
public class UpdatePolicyConditionDto
{
    [Required(ErrorMessage = "El tipo de atributo es requerido")]
    [MaxLength(50)]
    public string AttributeType { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave del atributo es requerida")]
    [MaxLength(100)]
    public string AttributeKey { get; set; } = string.Empty;

    [Required]
    public OperatorType Operator { get; set; }

    [Required(ErrorMessage = "El valor esperado es requerido")]
    [MaxLength(500)]
    public string ExpectedValue { get; set; } = string.Empty;
}
