using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para estadísticas y métricas de una política ABAC.
/// Proporciona información resumida sobre el uso y configuración de una política.
/// </summary>
public class PolicyStatisticsDto
{
    /// <summary>
    /// ID de la política.
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Nombre de la política.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Número de condiciones configuradas en la política.
    /// </summary>
    public int ConditionsCount { get; set; }

    /// <summary>
    /// Número de acciones asociadas a la política.
    /// </summary>
    public int ActionsCount { get; set; }

    /// <summary>
    /// Indica si la política está activa.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Prioridad de la política.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Efecto de la política (Permit o Deny).
    /// </summary>
    public PolicyEffect Effect { get; set; }

    /// <summary>
    /// Número de veces que la política ha sido registrada en el log de auditoría.
    /// </summary>
    public int AccessLogsCount { get; set; }

    /// <summary>
    /// Fecha y hora de la última evaluación de esta política (si existe).
    /// </summary>
    public DateTime? LastEvaluated { get; set; }
}
