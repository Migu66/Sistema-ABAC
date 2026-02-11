namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para atributos asignados a recursos.
/// </summary>
public class ResourceAttributeDto
{
    public Guid ResourceId { get; set; }
    public Guid AttributeId { get; set; }

    public string AttributeName { get; set; } = string.Empty;
    public string AttributeKey { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
