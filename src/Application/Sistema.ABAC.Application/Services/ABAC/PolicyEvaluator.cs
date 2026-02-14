using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Domain.Interfaces;

namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Implementación del evaluador de políticas ABAC.
/// En esta fase se enfoca en localizar políticas activas aplicables al contexto.
/// </summary>
public class PolicyEvaluator : IPolicyEvaluator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConditionEvaluator _conditionEvaluator;
    private readonly IAuditService _auditService;
    private readonly ILogger<PolicyEvaluator> _logger;

    public PolicyEvaluator(
        IUnitOfWork unitOfWork,
        IConditionEvaluator conditionEvaluator,
        IAuditService auditService,
        ILogger<PolicyEvaluator> logger)
    {
        _unitOfWork = unitOfWork;
        _conditionEvaluator = conditionEvaluator;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> EvaluateAsync(EvaluationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var candidatePolicies = await GetCandidatePoliciesAsync(context, cancellationToken);

        _logger.LogInformation(
            "Se encontraron {Count} políticas activas candidatas para evaluación ABAC.",
            candidatePolicies.Count);

        if (candidatePolicies.Count == 0)
        {
            await LogDecisionAsync(
                context,
                false,
                "No hay políticas activas candidatas para la acción solicitada.",
                null,
                cancellationToken);
            return false;
        }

        var applicablePolicies = new List<Policy>();

        foreach (var policy in candidatePolicies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var allConditionsMatched = await AreAllConditionsMatchedAsync(policy, context, cancellationToken);

            if (allConditionsMatched)
            {
                applicablePolicies.Add(policy);
            }
        }

        _logger.LogInformation(
            "Se encontraron {Count} políticas activas aplicables al contexto.",
            applicablePolicies.Count);

        var combiningStrategy = ResolveCombiningStrategy(context);
        var finalDecision = DetermineDecision(applicablePolicies, combiningStrategy);
        var decisivePolicy = GetDecisivePolicy(applicablePolicies, combiningStrategy, finalDecision);
        var reason = BuildDecisionReason(finalDecision, combiningStrategy, applicablePolicies.Count, decisivePolicy);

        _logger.LogInformation(
            "Decisión ABAC final: {Decision}. Estrategia: {Strategy}. Políticas aplicables: {Count}",
            finalDecision,
            combiningStrategy,
            applicablePolicies.Count);

        await LogDecisionAsync(
            context,
            finalDecision,
            reason,
            decisivePolicy,
            cancellationToken);

        return finalDecision;
    }

    private async Task<List<Policy>> GetCandidatePoliciesAsync(
        EvaluationContext context,
        CancellationToken cancellationToken)
    {
        var actionId = TryGetActionId(context.Action);
        if (actionId.HasValue)
        {
            var policiesByAction = await _unitOfWork.Policies
                .GetActivePoliciesForActionAsync(actionId.Value, cancellationToken);

            return policiesByAction.ToList();
        }

        var activePolicies = await _unitOfWork.Policies.GetActivePoliciesAsync(cancellationToken);
        var actionCode = TryGetActionCode(context.Action);
        if (string.IsNullOrWhiteSpace(actionCode))
        {
            return activePolicies.ToList();
        }

        return activePolicies
            .Where(policy => policy.PolicyActions.Any(policyAction =>
                policyAction.Action?.Code != null &&
                policyAction.Action.Code.Equals(actionCode, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private async Task<bool> AreAllConditionsMatchedAsync(
        Policy policy,
        EvaluationContext context,
        CancellationToken cancellationToken)
    {
        // Lógica AND explícita: una política aplica solo si TODAS sus condiciones son verdaderas.
        // Política sin condiciones se considera no aplicable por seguridad.
        if (policy.Conditions == null || policy.Conditions.Count == 0)
        {
            return false;
        }

        foreach (var condition in policy.Conditions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var conditionMatched = await _conditionEvaluator.EvaluateAsync(condition, context, cancellationToken);
            if (!conditionMatched)
            {
                return false;
            }
        }

        return true;
    }

    private static Guid? TryGetActionId(IDictionary<string, object?> actionAttributes)
    {
        if (actionAttributes.TryGetValue("actionId", out var actionIdValue) ||
            actionAttributes.TryGetValue("id", out actionIdValue))
        {
            if (actionIdValue is Guid guidValue)
            {
                return guidValue;
            }

            if (actionIdValue is string actionIdString && Guid.TryParse(actionIdString, out var parsedGuid))
            {
                return parsedGuid;
            }
        }

        return null;
    }

    private static string? TryGetActionCode(IDictionary<string, object?> actionAttributes)
    {
        if (actionAttributes.TryGetValue("code", out var actionCodeValue) ||
            actionAttributes.TryGetValue("actionCode", out actionCodeValue))
        {
            return actionCodeValue?.ToString();
        }

        return null;
    }

    private static PolicyCombiningStrategy ResolveCombiningStrategy(EvaluationContext context)
    {
        if (context.Environment.TryGetValue("combiningStrategy", out var strategyValue) ||
            context.Environment.TryGetValue("policyCombiningStrategy", out strategyValue))
        {
            var strategyText = strategyValue?.ToString();
            if (!string.IsNullOrWhiteSpace(strategyText) &&
                Enum.TryParse<PolicyCombiningStrategy>(strategyText, true, out var parsedStrategy))
            {
                return parsedStrategy;
            }
        }

        // Estrategia por defecto segura.
        return PolicyCombiningStrategy.DenyOverrides;
    }

    private static bool DetermineDecision(
        IReadOnlyCollection<Policy> applicablePolicies,
        PolicyCombiningStrategy combiningStrategy)
    {
        if (applicablePolicies.Count == 0)
        {
            return false;
        }

        var hasPermit = applicablePolicies.Any(policy => policy.Effect == PolicyEffect.Permit);
        var hasDeny = applicablePolicies.Any(policy => policy.Effect == PolicyEffect.Deny);

        return combiningStrategy switch
        {
            PolicyCombiningStrategy.PermitOverrides => hasPermit,
            _ => !hasDeny && hasPermit
        };
    }

    private static Policy? GetDecisivePolicy(
        IReadOnlyCollection<Policy> applicablePolicies,
        PolicyCombiningStrategy combiningStrategy,
        bool finalDecision)
    {
        if (applicablePolicies.Count == 0)
        {
            return null;
        }

        return combiningStrategy switch
        {
            PolicyCombiningStrategy.PermitOverrides when finalDecision => applicablePolicies
                .Where(policy => policy.Effect == PolicyEffect.Permit)
                .OrderByDescending(policy => policy.Priority)
                .FirstOrDefault(),

            PolicyCombiningStrategy.PermitOverrides => applicablePolicies
                .Where(policy => policy.Effect == PolicyEffect.Deny)
                .OrderByDescending(policy => policy.Priority)
                .FirstOrDefault(),

            _ when !finalDecision => applicablePolicies
                .Where(policy => policy.Effect == PolicyEffect.Deny)
                .OrderByDescending(policy => policy.Priority)
                .FirstOrDefault(),

            _ => applicablePolicies
                .Where(policy => policy.Effect == PolicyEffect.Permit)
                .OrderByDescending(policy => policy.Priority)
                .FirstOrDefault()
        };
    }

    private static string BuildDecisionReason(
        bool finalDecision,
        PolicyCombiningStrategy combiningStrategy,
        int applicablePoliciesCount,
        Policy? decisivePolicy)
    {
        var decisionText = finalDecision ? "Permit" : "Deny";
        if (decisivePolicy == null)
        {
            return $"Decisión {decisionText} con estrategia {combiningStrategy}. Políticas aplicables: {applicablePoliciesCount}.";
        }

        return $"Decisión {decisionText} con estrategia {combiningStrategy}. Política decisiva: {decisivePolicy.Name} ({decisivePolicy.Id}).";
    }

    private async Task LogDecisionAsync(
        EvaluationContext context,
        bool finalDecision,
        string reason,
        Policy? decisivePolicy,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = TryGetGuidFromAttributes(context.Subject, "userId", "id");
            if (!userId.HasValue)
            {
                return;
            }

            var resourceId = TryGetGuidFromAttributes(context.Resource, "resourceId", "id");
            var actionId = TryGetGuidFromAttributes(context.Action, "actionId", "id");
            var ipAddress = context.Environment.TryGetValue("ipAddress", out var ip) ? ip?.ToString() : null;

            await _auditService.LogAccessEvaluationAsync(
                userId: userId.Value,
                resourceId: resourceId,
                actionId: actionId,
                result: finalDecision ? "Permit" : "Deny",
                reason: reason,
                policyId: decisivePolicy?.Id,
                context: null,
                ipAddress: ipAddress,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible registrar auditoría automática de decisión ABAC.");
        }
    }

    private static Guid? TryGetGuidFromAttributes(IDictionary<string, object?> attributes, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!attributes.TryGetValue(key, out var value) || value == null)
            {
                continue;
            }

            if (value is Guid guidValue)
            {
                return guidValue;
            }

            if (Guid.TryParse(value.ToString(), out var parsedGuid))
            {
                return parsedGuid;
            }
        }

        return null;
    }

    private enum PolicyCombiningStrategy
    {
        DenyOverrides,
        PermitOverrides
    }
}