using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Define el contrato para evaluar condiciones individuales de políticas ABAC.
/// </summary>
public interface IConditionEvaluator
{
    /// <summary>
    /// Evalúa una condición de política sobre un contexto de evaluación.
    /// </summary>
    /// <param name="condition">Condición a evaluar</param>
    /// <param name="context">Contexto con atributos de sujeto, recurso, acción y entorno</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la condición se cumple; en caso contrario, false</returns>
    Task<bool> EvaluateAsync(
        PolicyCondition condition,
        EvaluationContext context,
        CancellationToken cancellationToken = default);
}