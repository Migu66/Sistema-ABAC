using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Domain.Interfaces;

namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Implementación de la fachada principal para evaluación de acceso ABAC.
/// </summary>
public class AccessControlService : IAccessControlService
{
    private readonly IAttributeCollectorService _attributeCollectorService;
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccessControlService> _logger;

    public AccessControlService(
        IAttributeCollectorService attributeCollectorService,
        IPolicyEvaluator policyEvaluator,
        IUnitOfWork unitOfWork,
        ILogger<AccessControlService> logger)
    {
        _attributeCollectorService = attributeCollectorService;
        _policyEvaluator = policyEvaluator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthorizationResult> CheckAccessAsync(
        Guid userId,
        Guid resourceId,
        Guid actionId,
        IDictionary<string, object?>? context = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Evaluando acceso ABAC para UserId={UserId}, ResourceId={ResourceId}, ActionId={ActionId}",
            userId,
            resourceId,
            actionId);

        var action = await _unitOfWork.Actions.GetByIdAsync(actionId, cancellationToken);
        if (action == null || action.IsDeleted)
        {
            throw new NotFoundException("Acción", actionId);
        }

        var subjectAttributes = await _attributeCollectorService
            .CollectSubjectAttributesAsync(userId, cancellationToken);
        subjectAttributes["userId"] = userId;

        var resourceAttributes = await _attributeCollectorService
            .CollectResourceAttributesAsync(resourceId, cancellationToken);
        resourceAttributes["resourceId"] = resourceId;

        var environmentAttributes = await _attributeCollectorService
            .CollectEnvironmentAttributesAsync(context, cancellationToken);

        var actionAttributes = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["actionId"] = action.Id,
            ["id"] = action.Id,
            ["actionCode"] = action.Code,
            ["code"] = action.Code,
            ["name"] = action.Name
        };

        var evaluationContext = new EvaluationContext(
            subject: subjectAttributes,
            resource: resourceAttributes,
            action: actionAttributes,
            environment: environmentAttributes);

        var isAllowed = await _policyEvaluator.EvaluateAsync(evaluationContext, cancellationToken);

        var result = new AuthorizationResult
        {
            Decision = isAllowed ? AuthorizationDecision.Permit : AuthorizationDecision.Deny,
            Reason = isAllowed
                ? "Acceso permitido por políticas ABAC aplicables."
                : "Acceso denegado: no se encontraron políticas ABAC aplicables que permitan la operación.",
            AppliedPolicies = new List<AppliedPolicyResult>()
        };

        _logger.LogInformation(
            "Resultado de acceso ABAC para UserId={UserId}, ResourceId={ResourceId}, ActionId={ActionId}: {Decision}",
            userId,
            resourceId,
            actionId,
            result.Decision);

        return result;
    }
}