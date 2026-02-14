using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para solicitudes de evaluación de acceso ABAC.
/// </summary>
public class EvaluateAccessRequest
{
    /// <summary>
    /// ID del usuario a evaluar.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// ID del recurso al que se quiere acceder.
    /// </summary>
    [Required]
    public Guid ResourceId { get; set; }

    /// <summary>
    /// ID de la acción solicitada.
    /// </summary>
    [Required]
    public Guid ActionId { get; set; }

    /// <summary>
    /// Atributos opcionales de contexto para la evaluación.
    /// </summary>
    public Dictionary<string, object?>? Context { get; set; }
}