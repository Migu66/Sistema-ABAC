using Microsoft.Extensions.Logging;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using System.Globalization;
using System.Linq.Dynamic.Core;

namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Implementación del evaluador de condiciones individuales ABAC.
/// </summary>
public class ConditionEvaluator : IConditionEvaluator
{
    private readonly ILogger<ConditionEvaluator> _logger;

    public ConditionEvaluator(ILogger<ConditionEvaluator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<bool> EvaluateAsync(
        PolicyCondition condition,
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(context);

        var sourceAttributes = GetAttributeSource(condition.AttributeType, context);
        if (sourceAttributes == null)
        {
            _logger.LogWarning(
                "No se pudo evaluar la condición {ConditionId}: AttributeType '{AttributeType}' no soportado.",
                condition.Id,
                condition.AttributeType);
            return Task.FromResult(false);
        }

        if (!sourceAttributes.TryGetValue(condition.AttributeKey, out var actualValue))
        {
            _logger.LogDebug(
                "No se encontró la clave de atributo '{AttributeKey}' para la condición {ConditionId}.",
                condition.AttributeKey,
                condition.Id);
            return Task.FromResult(false);
        }

        var result = condition.Operator switch
        {
            OperatorType.Equals => EvaluateEquals(actualValue, condition.ExpectedValue),
            OperatorType.NotEquals => !EvaluateEquals(actualValue, condition.ExpectedValue),
            OperatorType.GreaterThan =>
                (CompareValues(actualValue, condition.ExpectedValue, out var gtComparison) && gtComparison > 0) ||
                EvaluateDynamicComparison(actualValue, condition.ExpectedValue, ">"),
            OperatorType.LessThan =>
                (CompareValues(actualValue, condition.ExpectedValue, out var ltComparison) && ltComparison < 0) ||
                EvaluateDynamicComparison(actualValue, condition.ExpectedValue, "<"),
            OperatorType.Contains => EvaluateContains(actualValue, condition.ExpectedValue),
            OperatorType.In => EvaluateIn(actualValue, condition.ExpectedValue),
            OperatorType.NotIn => !EvaluateIn(actualValue, condition.ExpectedValue),
            _ => false
        };

        _logger.LogDebug(
            "Condición {ConditionId} evaluada. Atributo: {AttributeType}.{AttributeKey}, Operador: {Operator}, Resultado: {Result}",
            condition.Id,
            condition.AttributeType,
            condition.AttributeKey,
            condition.Operator,
            result);

        return Task.FromResult(result);
    }

    private static IDictionary<string, object?>? GetAttributeSource(string? attributeType, EvaluationContext context)
    {
        if (string.IsNullOrWhiteSpace(attributeType))
        {
            return null;
        }

        if (attributeType.Equals("Subject", StringComparison.OrdinalIgnoreCase))
        {
            return context.Subject;
        }

        if (attributeType.Equals("Resource", StringComparison.OrdinalIgnoreCase))
        {
            return context.Resource;
        }

        if (attributeType.Equals("Environment", StringComparison.OrdinalIgnoreCase))
        {
            return context.Environment;
        }

        if (attributeType.Equals("Action", StringComparison.OrdinalIgnoreCase))
        {
            return context.Action;
        }

        return null;
    }

    private static bool EvaluateEquals(object? actualValue, string expectedValue)
    {
        if (actualValue == null)
        {
            return string.IsNullOrWhiteSpace(expectedValue);
        }

        if (actualValue is string actualString)
        {
            return string.Equals(actualString, expectedValue, StringComparison.OrdinalIgnoreCase);
        }

        if (TryGetBoolean(actualValue, out var actualBool) && bool.TryParse(expectedValue, out var expectedBool))
        {
            return actualBool == expectedBool;
        }

        if (TryGetDecimal(actualValue, out var actualNumber) &&
            decimal.TryParse(expectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var expectedNumber))
        {
            return actualNumber == expectedNumber;
        }

        if (TryGetDateTime(actualValue, out var actualDate) &&
            DateTime.TryParse(expectedValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var expectedDate))
        {
            return actualDate == expectedDate;
        }

        return string.Equals(
            Convert.ToString(actualValue, CultureInfo.InvariantCulture),
            expectedValue,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool CompareValues(object? actualValue, string expectedValue, out int comparison)
    {
        comparison = 0;

        if (actualValue == null)
        {
            return false;
        }

        if (TryGetDecimal(actualValue, out var actualNumber) &&
            decimal.TryParse(expectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var expectedNumber))
        {
            comparison = actualNumber.CompareTo(expectedNumber);
            return true;
        }

        if (TryGetDateTime(actualValue, out var actualDate) &&
            DateTime.TryParse(expectedValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var expectedDate))
        {
            comparison = actualDate.CompareTo(expectedDate);
            return true;
        }

        return false;
    }

    private static bool EvaluateContains(object? actualValue, string expectedValue)
    {
        if (actualValue == null)
        {
            return false;
        }

        var actualString = Convert.ToString(actualValue, CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(actualString))
        {
            return false;
        }

        var normalizedActual = actualString.ToLowerInvariant();
        var normalizedExpected = expectedValue.ToLowerInvariant();

        var source = new[] { normalizedActual }.AsQueryable();
        return source.Any("it.Contains(@0)", normalizedExpected);
    }

    private static bool EvaluateIn(object? actualValue, string expectedValues)
    {
        if (actualValue == null || string.IsNullOrWhiteSpace(expectedValues))
        {
            return false;
        }

        var candidates = expectedValues
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var actualString = Convert.ToString(actualValue, CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(actualString))
        {
            return false;
        }

        var normalizedCandidates = candidates
            .Select(candidate => candidate.ToLowerInvariant())
            .AsQueryable();

        var normalizedActual = actualString.ToLowerInvariant();
        if (normalizedCandidates.Any("it == @0", normalizedActual))
        {
            return true;
        }

        return candidates.Any(candidate => EvaluateEquals(actualValue, candidate));
    }

    private static bool EvaluateDynamicComparison(object? actualValue, string expectedValue, string @operator)
    {
        if (actualValue == null)
        {
            return false;
        }

        var actualString = Convert.ToString(actualValue, CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(actualString))
        {
            return false;
        }

        var source = new[] { new DynamicComparisonItem { Actual = actualString } }.AsQueryable();
        return source.Any($"Actual {@operator} @0", expectedValue);
    }

    private static bool TryGetDecimal(object value, out decimal number)
    {
        if (value is decimal decimalValue)
        {
            number = decimalValue;
            return true;
        }

        if (value is int intValue)
        {
            number = intValue;
            return true;
        }

        if (value is long longValue)
        {
            number = longValue;
            return true;
        }

        if (value is double doubleValue)
        {
            number = (decimal)doubleValue;
            return true;
        }

        if (value is float floatValue)
        {
            number = (decimal)floatValue;
            return true;
        }

        if (value is string stringValue &&
            decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
        {
            number = parsedValue;
            return true;
        }

        number = 0;
        return false;
    }

    private static bool TryGetDateTime(object value, out DateTime date)
    {
        if (value is DateTime dateValue)
        {
            date = dateValue;
            return true;
        }

        if (value is string stringValue &&
            DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedDate))
        {
            date = parsedDate;
            return true;
        }

        date = default;
        return false;
    }

    private static bool TryGetBoolean(object value, out bool boolean)
    {
        if (value is bool boolValue)
        {
            boolean = boolValue;
            return true;
        }

        if (value is string stringValue && bool.TryParse(stringValue, out var parsedBool))
        {
            boolean = parsedBool;
            return true;
        }

        boolean = false;
        return false;
    }

    private sealed class DynamicComparisonItem
    {
        public string Actual { get; init; } = string.Empty;
    }
}