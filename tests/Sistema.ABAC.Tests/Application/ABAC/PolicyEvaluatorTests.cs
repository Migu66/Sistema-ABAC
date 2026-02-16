using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Domain.Interfaces;

namespace Sistema.ABAC.Tests.Application.ABAC;

public class PolicyEvaluatorTests
{
    [Fact]
    public async Task EvaluateAsync_WhenNoCandidatePolicies_ReturnsFalse()
    {
        var actionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var policyRepositoryMock = new Mock<IPolicyRepository>();
        var conditionEvaluatorMock = new Mock<IConditionEvaluator>();
        var auditServiceMock = CreateAuditServiceMock();

        policyRepositoryMock
            .Setup(x => x.GetActivePoliciesForActionAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Policy>());

        unitOfWorkMock.SetupGet(x => x.Policies).Returns(policyRepositoryMock.Object);

        var sut = new PolicyEvaluator(
            unitOfWorkMock.Object,
            conditionEvaluatorMock.Object,
            auditServiceMock.Object,
            NullLogger<PolicyEvaluator>.Instance);

        var context = BuildContext(userId: userId, actionId: actionId);

        var result = await sut.EvaluateAsync(context);

        result.Should().BeFalse();
        conditionEvaluatorMock.Verify(
            x => x.EvaluateAsync(It.IsAny<PolicyCondition>(), It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
        auditServiceMock.Verify(
            x => x.LogAccessEvaluationAsync(
                userId,
                It.IsAny<Guid?>(),
                actionId,
                "Deny",
                It.Is<string>(reason => reason.Contains("No hay políticas activas candidatas", StringComparison.OrdinalIgnoreCase)),
                null,
                null,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_WithDenyOverridesAndDenyApplicable_ReturnsFalse()
    {
        var actionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var permitCondition = NewCondition();
        var denyCondition = NewCondition();

        var permitPolicy = NewPolicy("PermitPolicy", PolicyEffect.Permit, 100, permitCondition);
        var denyPolicy = NewPolicy("DenyPolicy", PolicyEffect.Deny, 200, denyCondition);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var policyRepositoryMock = new Mock<IPolicyRepository>();
        var conditionEvaluatorMock = new Mock<IConditionEvaluator>();
        var auditServiceMock = CreateAuditServiceMock();

        policyRepositoryMock
            .Setup(x => x.GetActivePoliciesForActionAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { permitPolicy, denyPolicy });

        conditionEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<PolicyCondition>(), It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()))
            .Returns((PolicyCondition c, EvaluationContext _, CancellationToken _) =>
                Task.FromResult(c.Id == permitCondition.Id || c.Id == denyCondition.Id));

        unitOfWorkMock.SetupGet(x => x.Policies).Returns(policyRepositoryMock.Object);

        var sut = new PolicyEvaluator(
            unitOfWorkMock.Object,
            conditionEvaluatorMock.Object,
            auditServiceMock.Object,
            NullLogger<PolicyEvaluator>.Instance);

        var context = BuildContext(userId: userId, actionId: actionId);

        var result = await sut.EvaluateAsync(context);

        result.Should().BeFalse();
        auditServiceMock.Verify(
            x => x.LogAccessEvaluationAsync(
                userId,
                It.IsAny<Guid?>(),
                actionId,
                "Deny",
                It.IsAny<string?>(),
                denyPolicy.Id,
                null,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_WithPermitOverridesAndPermitApplicable_ReturnsTrue()
    {
        var actionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var permitLowCondition = NewCondition();
        var permitHighCondition = NewCondition();
        var denyCondition = NewCondition();

        var permitLow = NewPolicy("PermitLow", PolicyEffect.Permit, 100, permitLowCondition);
        var permitHigh = NewPolicy("PermitHigh", PolicyEffect.Permit, 500, permitHighCondition);
        var deny = NewPolicy("Deny", PolicyEffect.Deny, 300, denyCondition);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var policyRepositoryMock = new Mock<IPolicyRepository>();
        var conditionEvaluatorMock = new Mock<IConditionEvaluator>();
        var auditServiceMock = CreateAuditServiceMock();

        policyRepositoryMock
            .Setup(x => x.GetActivePoliciesForActionAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { permitLow, permitHigh, deny });

        conditionEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<PolicyCondition>(), It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        unitOfWorkMock.SetupGet(x => x.Policies).Returns(policyRepositoryMock.Object);

        var sut = new PolicyEvaluator(
            unitOfWorkMock.Object,
            conditionEvaluatorMock.Object,
            auditServiceMock.Object,
            NullLogger<PolicyEvaluator>.Instance);

        var context = BuildContext(
            userId: userId,
            actionId: actionId,
            environment: new Dictionary<string, object?> { ["combiningStrategy"] = "PermitOverrides" });

        var result = await sut.EvaluateAsync(context);

        result.Should().BeTrue();
        auditServiceMock.Verify(
            x => x.LogAccessEvaluationAsync(
                userId,
                It.IsAny<Guid?>(),
                actionId,
                "Permit",
                It.IsAny<string?>(),
                permitHigh.Id,
                null,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_WhenPolicyHasNoConditions_ItIsNotApplicable()
    {
        var actionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var policyWithoutConditions = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "NoConditions",
            Effect = PolicyEffect.Permit,
            Priority = 10,
            IsActive = true,
            Conditions = new List<PolicyCondition>()
        };

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var policyRepositoryMock = new Mock<IPolicyRepository>();
        var conditionEvaluatorMock = new Mock<IConditionEvaluator>();
        var auditServiceMock = CreateAuditServiceMock();

        policyRepositoryMock
            .Setup(x => x.GetActivePoliciesForActionAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { policyWithoutConditions });

        unitOfWorkMock.SetupGet(x => x.Policies).Returns(policyRepositoryMock.Object);

        var sut = new PolicyEvaluator(
            unitOfWorkMock.Object,
            conditionEvaluatorMock.Object,
            auditServiceMock.Object,
            NullLogger<PolicyEvaluator>.Instance);

        var context = BuildContext(userId: userId, actionId: actionId);

        var result = await sut.EvaluateAsync(context);

        result.Should().BeFalse();
        conditionEvaluatorMock.Verify(
            x => x.EvaluateAsync(It.IsAny<PolicyCondition>(), It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_WhenActionIdNotPresent_FiltersByActionCodeFromActivePolicies()
    {
        var userId = Guid.NewGuid();
        var matchingCondition = NewCondition();
        var nonMatchingCondition = NewCondition();

        var matchingPolicy = NewPolicy("MatchByCode", PolicyEffect.Permit, 100, matchingCondition);
        matchingPolicy.PolicyActions = new List<PolicyAction>
        {
            new()
            {
                PolicyId = matchingPolicy.Id,
                Action = new Domain.Entities.Action { Id = Guid.NewGuid(), Code = "read" }
            }
        };

        var nonMatchingPolicy = NewPolicy("NonMatchByCode", PolicyEffect.Deny, 200, nonMatchingCondition);
        nonMatchingPolicy.PolicyActions = new List<PolicyAction>
        {
            new()
            {
                PolicyId = nonMatchingPolicy.Id,
                Action = new Domain.Entities.Action { Id = Guid.NewGuid(), Code = "write" }
            }
        };

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var policyRepositoryMock = new Mock<IPolicyRepository>();
        var conditionEvaluatorMock = new Mock<IConditionEvaluator>();
        var auditServiceMock = CreateAuditServiceMock();

        policyRepositoryMock
            .Setup(x => x.GetActivePoliciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { matchingPolicy, nonMatchingPolicy });

        conditionEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<PolicyCondition>(), It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()))
            .Returns((PolicyCondition c, EvaluationContext _, CancellationToken _) =>
                Task.FromResult(c.Id == matchingCondition.Id));

        unitOfWorkMock.SetupGet(x => x.Policies).Returns(policyRepositoryMock.Object);

        var sut = new PolicyEvaluator(
            unitOfWorkMock.Object,
            conditionEvaluatorMock.Object,
            auditServiceMock.Object,
            NullLogger<PolicyEvaluator>.Instance);

        var context = BuildContext(
            userId: userId,
            actionCode: "read");

        var result = await sut.EvaluateAsync(context);

        result.Should().BeTrue();
        conditionEvaluatorMock.Verify(
            x => x.EvaluateAsync(
                It.Is<PolicyCondition>(condition => condition.Id == matchingCondition.Id),
                It.IsAny<EvaluationContext>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        conditionEvaluatorMock.Verify(
            x => x.EvaluateAsync(
                It.Is<PolicyCondition>(condition => condition.Id == nonMatchingCondition.Id),
                It.IsAny<EvaluationContext>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var conditionEvaluatorMock = new Mock<IConditionEvaluator>();
        var auditServiceMock = CreateAuditServiceMock();

        var sut = new PolicyEvaluator(
            unitOfWorkMock.Object,
            conditionEvaluatorMock.Object,
            auditServiceMock.Object,
            NullLogger<PolicyEvaluator>.Instance);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var action = async () => await sut.EvaluateAsync(BuildContext(userId: Guid.NewGuid()), cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    private static Mock<IAuditService> CreateAuditServiceMock()
    {
        var mock = new Mock<IAuditService>();
        mock.Setup(x => x.LogAccessEvaluationAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessLogDto());

        return mock;
    }

    private static Policy NewPolicy(string name, PolicyEffect effect, int priority, params PolicyCondition[] conditions)
    {
        return new Policy
        {
            Id = Guid.NewGuid(),
            Name = name,
            Effect = effect,
            Priority = priority,
            IsActive = true,
            Conditions = conditions.ToList()
        };
    }

    private static PolicyCondition NewCondition()
    {
        return new PolicyCondition
        {
            Id = Guid.NewGuid(),
            PolicyId = Guid.NewGuid(),
            AttributeType = "Subject",
            AttributeKey = "role",
            Operator = OperatorType.Equals,
            ExpectedValue = "admin"
        };
    }

    private static EvaluationContext BuildContext(
        Guid userId,
        Guid? actionId = null,
        string? actionCode = null,
        IDictionary<string, object?>? environment = null)
    {
        var action = new Dictionary<string, object?>();
        if (actionId.HasValue)
        {
            action["actionId"] = actionId.Value;
        }

        if (!string.IsNullOrWhiteSpace(actionCode))
        {
            action["code"] = actionCode;
        }

        return new EvaluationContext(
            subject: new Dictionary<string, object?> { ["userId"] = userId },
            resource: new Dictionary<string, object?> { ["resourceId"] = Guid.NewGuid() },
            action: action,
            environment: environment ?? new Dictionary<string, object?>());
    }
}
