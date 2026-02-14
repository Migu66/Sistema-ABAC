namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Define el contrato del evaluador de políticas ABAC.
/// </summary>
public interface IPolicyEvaluator
{
    /// <summary>
    /// Evalúa el contexto ABAC contra las políticas activas y determina si se permite el acceso.
    /// </summary>
    /// <param name="context">Contexto de evaluación con sujeto, recurso, acción y entorno</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el acceso es permitido; en caso contrario, false</returns>
    Task<bool> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default);
}