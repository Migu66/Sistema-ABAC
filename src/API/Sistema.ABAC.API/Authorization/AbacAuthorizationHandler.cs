using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Interfaces;

namespace Sistema.ABAC.API.Authorization;

/// <summary>
/// Requirement base para autorización ABAC.
/// </summary>
public class AbacRequirement : IAuthorizationRequirement
{
    public Guid? ResourceId { get; }
    public Guid? ActionId { get; }

    public AbacRequirement(Guid? resourceId = null, Guid? actionId = null)
    {
        ResourceId = resourceId;
        ActionId = actionId;
    }
}

/// <summary>
/// Handler de autorización ABAC que delega la decisión al AccessControlService.
/// </summary>
public class AbacAuthorizationHandler : AuthorizationHandler<AbacRequirement>
{
    private readonly IAccessControlService _accessControlService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AbacAuthorizationHandler> _logger;

    public AbacAuthorizationHandler(
        IAccessControlService accessControlService,
        IUnitOfWork unitOfWork,
        ILogger<AbacAuthorizationHandler> logger)
    {
        _accessControlService = accessControlService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AbacRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = GetUserId(context.User);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No se pudo resolver el UserId desde los claims para autorización ABAC.");
            return;
        }

        var httpContext = (context.Resource as HttpContext)
            ?? (context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)?.HttpContext;

        if (httpContext == null)
        {
            _logger.LogWarning("No se pudo resolver HttpContext en AbacAuthorizationHandler.");
            return;
        }

        var resourceId = requirement.ResourceId ?? ResolveResourceId(httpContext);
        var actionId = requirement.ActionId ?? await ResolveActionIdAsync(httpContext);

        if (!resourceId.HasValue || !actionId.HasValue)
        {
            _logger.LogWarning(
                "No se pudo resolver ResourceId o ActionId para autorización ABAC. ResourceId={ResourceId}, ActionId={ActionId}",
                resourceId,
                actionId);
            return;
        }

        var environmentContext = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["ipAddress"] = httpContext.Connection.RemoteIpAddress?.ToString(),
            ["requestMethod"] = httpContext.Request.Method,
            ["requestPath"] = httpContext.Request.Path.Value,
            ["userAgent"] = httpContext.Request.Headers.UserAgent.ToString()
        };

        var authorizationResult = await _accessControlService.CheckAccessAsync(
            userId.Value,
            resourceId.Value,
            actionId.Value,
            environmentContext,
            httpContext.RequestAborted);

        if (authorizationResult.Decision == AuthorizationDecision.Permit)
        {
            context.Succeed(requirement);
        }
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private static Guid? ResolveResourceId(HttpContext httpContext)
    {
        var routeValues = httpContext.Request.RouteValues;

        if (TryParseGuid(routeValues.TryGetValue("resourceId", out var resourceIdValue) ? resourceIdValue?.ToString() : null, out var resourceId) ||
            TryParseGuid(routeValues.TryGetValue("id", out var idValue) ? idValue?.ToString() : null, out resourceId) ||
            TryParseGuid(httpContext.Request.Query["resourceId"].ToString(), out resourceId))
        {
            return resourceId;
        }

        return null;
    }

    private async Task<Guid?> ResolveActionIdAsync(HttpContext httpContext)
    {
        var routeValues = httpContext.Request.RouteValues;
        if (TryParseGuid(routeValues.TryGetValue("actionId", out var actionIdValue) ? actionIdValue?.ToString() : null, out var actionId) ||
            TryParseGuid(httpContext.Request.Query["actionId"].ToString(), out actionId))
        {
            return actionId;
        }

        var actionCode = MapHttpMethodToActionCode(httpContext.Request.Method);
        if (string.IsNullOrWhiteSpace(actionCode))
        {
            return null;
        }

        var action = await _unitOfWork.Actions.GetByCodeAsync(actionCode, httpContext.RequestAborted);
        return action?.Id;
    }

    private static string? MapHttpMethodToActionCode(string httpMethod)
    {
        return httpMethod.ToUpperInvariant() switch
        {
            "GET" => "read",
            "POST" => "create",
            "PUT" => "update",
            "PATCH" => "update",
            "DELETE" => "delete",
            _ => null
        };
    }

    private static bool TryParseGuid(string? value, out Guid guid)
    {
        return Guid.TryParse(value, out guid);
    }
}