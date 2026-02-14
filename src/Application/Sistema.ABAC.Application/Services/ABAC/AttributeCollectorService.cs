using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Domain.Interfaces;

namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Implementación del servicio de recopilación de atributos para evaluación ABAC.
/// </summary>
public class AttributeCollectorService : IAttributeCollectorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttributeCollectorService> _logger;

    public AttributeCollectorService(
        IUnitOfWork unitOfWork,
        ILogger<AttributeCollectorService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, object?>> CollectSubjectAttributesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recopilando atributos del sujeto para usuario {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.IsDeleted)
        {
            throw new NotFoundException("Usuario", userId);
        }

        var activeAttributes = await _unitOfWork.Users.GetActiveAttributesAsync(
            userId,
            DateTime.UtcNow,
            cancellationToken);

        var subjectAttributes = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var userAttribute in activeAttributes)
        {
            var attribute = userAttribute.Attribute;
            var attributeKey = attribute?.Key;

            if (attribute == null || string.IsNullOrWhiteSpace(attributeKey))
            {
                continue;
            }

            subjectAttributes[attributeKey] = ConvertValue(userAttribute.Value, attribute.Type);
        }

        _logger.LogInformation(
            "Se recopilaron {Count} atributos activos para usuario {UserId}",
            subjectAttributes.Count,
            userId);

        return subjectAttributes;
    }

    /// <inheritdoc />
    public Task<IDictionary<string, object?>> CollectResourceAttributesAsync(
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Pendiente de implementación en el paso 95.");
    }

    /// <inheritdoc />
    public Task<IDictionary<string, object?>> CollectEnvironmentAttributesAsync(
        IDictionary<string, object?>? contextAttributes = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Pendiente de implementación en el paso 96.");
    }

    private object? ConvertValue(string? rawValue, AttributeType attributeType)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return rawValue;
        }

        try
        {
            return attributeType switch
            {
                AttributeType.Number when decimal.TryParse(rawValue, out var number) => number,
                AttributeType.Boolean when bool.TryParse(rawValue, out var boolean) => boolean,
                AttributeType.DateTime when DateTime.TryParse(rawValue, out var date) => date,
                _ => rawValue
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible convertir el valor '{Value}' para el tipo {Type}", rawValue, attributeType);
            return rawValue;
        }
    }
}