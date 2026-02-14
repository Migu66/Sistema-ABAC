using Microsoft.Extensions.Logging;
using Sistema.ABAC.Domain.Entities;
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
    private readonly ILogger<PolicyEvaluator> _logger;

    public PolicyEvaluator(
        IUnitOfWork unitOfWork,
        IConditionEvaluator conditionEvaluator,
        ILogger<PolicyEvaluator> logger)
    {
        _unitOfWork = unitOfWork;
        _conditionEvaluator = conditionEvaluator;
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
            return false;
        }

        var applicablePolicies = new List<Policy>();

        foreach (var policy in candidatePolicies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var allConditionsMatched = true;
            foreach (var condition in policy.Conditions)
            {
                var conditionMatched = await _conditionEvaluator.EvaluateAsync(condition, context, cancellationToken);
                if (!conditionMatched)
                {
                    allConditionsMatched = false;
                    break;
                }
            }

            if (allConditionsMatched)
            {
                applicablePolicies.Add(policy);
            }
        }

        _logger.LogInformation(
            "Se encontraron {Count} políticas activas aplicables al contexto.",
            applicablePolicies.Count);

        return applicablePolicies.Count > 0;
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
}