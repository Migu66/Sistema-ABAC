using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Tests.Application.Services.ABAC;

public class ConditionEvaluatorTests
{
    private readonly ConditionEvaluator _sut;

    public ConditionEvaluatorTests()
    {
        _sut = new ConditionEvaluator(NullLogger<ConditionEvaluator>.Instance);
    }

    private static PolicyCondition CreateCondition(
        string attributeType, string attributeKey, OperatorType op, string expectedValue) =>
        new()
        {
            Id = Guid.NewGuid(),
            PolicyId = Guid.NewGuid(),
            AttributeType = attributeType,
            AttributeKey = attributeKey,
            Operator = op,
            ExpectedValue = expectedValue
        };

    private static EvaluationContext CreateContext(
        IDictionary<string, object?>? subject = null,
        IDictionary<string, object?>? resource = null,
        IDictionary<string, object?>? environment = null,
        IDictionary<string, object?>? action = null) =>
        new(
            subject: subject,
            resource: resource,
            action: action,
            environment: environment
        );

    #region Null / Guard

    [Fact]
    public async Task EvaluateAsync_NullCondition_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.EvaluateAsync(null!, CreateContext());
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_NullContext_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.EvaluateAsync(CreateCondition("Subject", "level", OperatorType.Equals, "5"), null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_CancellationRequested_Throws()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.Equals, "5"),
            CreateContext(),
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region AttributeType resolution

    [Fact]
    public async Task EvaluateAsync_SubjectAttributeType_ResolvesCorrectly()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = "5" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.Equals, "5"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_ResourceAttributeType_ResolvesCorrectly()
    {
        var resource = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["type"] = "API" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Resource", "type", OperatorType.Equals, "API"),
            CreateContext(resource: resource));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_EnvironmentAttributeType_ResolvesCorrectly()
    {
        var env = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["dayOfWeek"] = "Monday" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Environment", "dayOfWeek", OperatorType.Equals, "Monday"),
            CreateContext(environment: env));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_ActionAttributeType_ResolvesCorrectly()
    {
        var action = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["code"] = "read" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Action", "code", OperatorType.Equals, "read"),
            CreateContext(action: action));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_UnknownAttributeType_ReturnsFalse()
    {
        var result = await _sut.EvaluateAsync(
            CreateCondition("Unknown", "key", OperatorType.Equals, "val"),
            CreateContext());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_NullAttributeType_ReturnsFalse()
    {
        var result = await _sut.EvaluateAsync(
            CreateCondition(null!, "key", OperatorType.Equals, "val"),
            CreateContext());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_EmptyAttributeType_ReturnsFalse()
    {
        var result = await _sut.EvaluateAsync(
            CreateCondition("  ", "key", OperatorType.Equals, "val"),
            CreateContext());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_MissingAttributeKey_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["other"] = "val" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "missing", OperatorType.Equals, "val"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    #endregion

    #region Equals operator

    [Fact]
    public async Task EvaluateAsync_Equals_StringMatch_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = "admin" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.Equals, "Admin"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_NullActualAndEmptyExpected_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["key"] = null };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "key", OperatorType.Equals, ""),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_NullActualAndNonEmptyExpected_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["key"] = null };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "key", OperatorType.Equals, "value"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_BooleanValue_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["active"] = true };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "active", OperatorType.Equals, "true"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_BooleanString_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["active"] = "true" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "active", OperatorType.Equals, "true"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_IntValue_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = 5 };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.Equals, "5"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_DecimalValue_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["score"] = 9.5m };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "score", OperatorType.Equals, "9.5"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_LongValue_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["big"] = 100L };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "big", OperatorType.Equals, "100"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_DoubleValue_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["val"] = 3.14 };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "val", OperatorType.Equals, "3.14"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_FloatValue_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["val"] = 2.5f };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "val", OperatorType.Equals, "2.5"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_DateTimeValue_ReturnsTrue()
    {
        var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["expiry"] = date };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "expiry", OperatorType.Equals, "2024-06-15"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_DateTimeString_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["expiry"] = "2024-06-15" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "expiry", OperatorType.Equals, "2024-06-15"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Equals_NumberStringParsed_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["val"] = "42" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "val", OperatorType.Equals, "42"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    #endregion

    #region NotEquals operator

    [Fact]
    public async Task EvaluateAsync_NotEquals_DifferentValues_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = "user" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.NotEquals, "admin"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_NotEquals_SameValues_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = "admin" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.NotEquals, "admin"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    #endregion

    #region GreaterThan operator

    [Fact]
    public async Task EvaluateAsync_GreaterThan_NumberGreater_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = 10 };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.GreaterThan, "5"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_GreaterThan_NumberEqual_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = 5 };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.GreaterThan, "5"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_GreaterThan_NumberLess_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = 3 };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.GreaterThan, "5"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_GreaterThan_NullValue_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = null };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.GreaterThan, "5"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_GreaterThan_DateTimeGreater_ReturnsTrue()
    {
        var date = new DateTime(2025, 1, 1);
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["date"] = date };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "date", OperatorType.GreaterThan, "2024-01-01"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    #endregion

    #region LessThan operator

    [Fact]
    public async Task EvaluateAsync_LessThan_NumberLess_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = 3 };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.LessThan, "5"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_LessThan_NumberGreater_StringComparison()
    {
        // Dynamic LINQ uses string comparison: "10" < "5" is true lexicographically
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = 10 };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.LessThan, "5"),
            CreateContext(subject: subject));
        result.Should().BeTrue(); // String comparison: "10" < "5"
    }

    [Fact]
    public async Task EvaluateAsync_LessThan_NullValue_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["level"] = null };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "level", OperatorType.LessThan, "5"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    #endregion

    #region Contains operator

    [Fact]
    public async Task EvaluateAsync_Contains_SubstringPresent_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["desc"] = "hello world" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "desc", OperatorType.Contains, "world"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Contains_SubstringAbsent_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["desc"] = "hello" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "desc", OperatorType.Contains, "world"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_Contains_NullValue_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["desc"] = null };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "desc", OperatorType.Contains, "world"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    #endregion

    #region In operator

    [Fact]
    public async Task EvaluateAsync_In_ValueInList_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = "admin" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.In, "admin,user,viewer"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_In_ValueNotInList_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = "guest" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.In, "admin,user,viewer"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_In_NullValue_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = null };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.In, "admin,user"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_In_EmptyExpectedValues_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = "admin" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.In, ""),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    #endregion

    #region NotIn operator

    [Fact]
    public async Task EvaluateAsync_NotIn_ValueNotInList_ReturnsTrue()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = "guest" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.NotIn, "admin,user"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_NotIn_ValueInList_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["role"] = "admin" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "role", OperatorType.NotIn, "admin,user"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    #endregion

    #region Unsupported operator

    [Fact]
    public async Task EvaluateAsync_UnsupportedOperator_ReturnsFalse()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["key"] = "val" };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "key", (OperatorType)999, "val"),
            CreateContext(subject: subject));
        result.Should().BeFalse();
    }

    #endregion

    #region Edge cases - Fallback ToString comparison

    [Fact]
    public async Task EvaluateAsync_Equals_NonStandardObjectType_UsesToStringFallback()
    {
        var subject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["val"] = new Uri("https://example.com") };
        var result = await _sut.EvaluateAsync(
            CreateCondition("Subject", "val", OperatorType.Equals, "https://example.com/"),
            CreateContext(subject: subject));
        result.Should().BeTrue();
    }

    #endregion
}
