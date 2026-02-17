using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Resultado de una evaluación de autorización ABAC.
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// Decisión final de autorización.
    /// </summary>
    public AuthorizationDecision Decision { get; set; } = AuthorizationDecision.Deny;

    /// <summary>
    /// Políticas que participaron en la decisión final.
    /// </summary>
    public List<AppliedPolicyResult> AppliedPolicies { get; set; } = new();

    /// <summary>
    /// Razón descriptiva de la decisión final.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Representa el estado final de una evaluación de autorización.
/// </summary>
public enum AuthorizationDecision
{
    Permit = 0,
    Deny = 1
}

/// <summary>
/// Información resumida de una política aplicada durante la evaluación.
/// </summary>
public class AppliedPolicyResult
{
    /// <summary>
    /// ID de la política aplicada.
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Nombre de la política aplicada.
    /// </summary>
    public string PolicyName { get; set; } = string.Empty;

    /// <summary>
    /// Efecto de la política (Permit o Deny).
    /// </summary>
    public PolicyEffect Effect { get; set; }

    /// <summary>
    /// Prioridad de la política aplicada.
    /// </summary>
    public int Priority { get; set; }
}