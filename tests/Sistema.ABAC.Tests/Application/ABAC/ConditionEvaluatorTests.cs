using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Tests.Application.ABAC;

public class ConditionEvaluatorTests
{
    private static readonly ConditionEvaluator Evaluator = new(NullLogger<ConditionEvaluator>.Instance);

    [Fact]
    public async Task EvaluateAsync_WhenEqualsOperatorAndSameString_IgnoresCaseAndReturnsTrue()
    {
        var condition = BuildCondition("Subject", "department", OperatorType.Equals, "ventas");
        var context = new EvaluationContext(subject: new Dictionary<string, object?> { ["Department"] = "Ventas" });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenNotEqualsOperatorAndDifferentValues_ReturnsTrue()
    {
        var condition = BuildCondition("Subject", "level", OperatorType.NotEquals, "10");
        var context = new EvaluationContext(subject: new Dictionary<string, object?> { ["level"] = 5 });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenGreaterThanOperatorWithNumericValues_ReturnsTrue()
    {
        var condition = BuildCondition("Resource", "requiredLevel", OperatorType.GreaterThan, "3");
        var context = new EvaluationContext(resource: new Dictionary<string, object?> { ["requiredLevel"] = 5 });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenLessThanOperatorWithDateValues_ReturnsTrue()
    {
        var condition = BuildCondition("Environment", "requestDate", OperatorType.LessThan, "2026-12-31");
        var context = new EvaluationContext(environment: new Dictionary<string, object?> { ["requestDate"] = new DateTime(2026, 1, 1) });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenContainsOperatorAndSubstringExists_ReturnsTrue()
    {
        var condition = BuildCondition("Action", "code", OperatorType.Contains, "read");
        var context = new EvaluationContext(action: new Dictionary<string, object?> { ["code"] = "resource_read_all" });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenInOperatorAndValueIsInList_ReturnsTrue()
    {
        var condition = BuildCondition("Subject", "role", OperatorType.In, "admin,manager,auditor");
        var context = new EvaluationContext(subject: new Dictionary<string, object?> { ["role"] = "Manager" });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenNotInOperatorAndValueNotPresent_ReturnsTrue()
    {
        var condition = BuildCondition("Resource", "classification", OperatorType.NotIn, "public,internal");
        var context = new EvaluationContext(resource: new Dictionary<string, object?> { ["classification"] = "confidential" });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenAttributeKeyDoesNotExist_ReturnsFalse()
    {
        var condition = BuildCondition("Subject", "missingKey", OperatorType.Equals, "value");
        var context = new EvaluationContext(subject: new Dictionary<string, object?> { ["otherKey"] = "value" });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WhenAttributeTypeIsUnsupported_ReturnsFalse()
    {
        var condition = BuildCondition("UnknownSource", "role", OperatorType.Equals, "admin");
        var context = new EvaluationContext(subject: new Dictionary<string, object?> { ["role"] = "admin" });

        var result = await Evaluator.EvaluateAsync(condition, context);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        var condition = BuildCondition("Subject", "role", OperatorType.Equals, "admin");
        var context = new EvaluationContext(subject: new Dictionary<string, object?> { ["role"] = "admin" });
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var action = async () => await Evaluator.EvaluateAsync(condition, context, cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    private static PolicyCondition BuildCondition(
        string attributeType,
        string attributeKey,
        OperatorType @operator,
        string expectedValue)
    {
        return new PolicyCondition
        {
            Id = Guid.NewGuid(),
            PolicyId = Guid.NewGuid(),
            AttributeType = attributeType,
            AttributeKey = attributeKey,
            Operator = @operator,
            ExpectedValue = expectedValue
        };
    }
}
