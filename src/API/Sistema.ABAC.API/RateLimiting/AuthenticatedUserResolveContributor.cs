using AspNetCoreRateLimit;
using System.Security.Claims;

namespace Sistema.ABAC.API.RateLimiting;

/// <summary>
/// Resuelve el identificador de cliente para rate limiting a partir del usuario autenticado.
/// </summary>
public class AuthenticatedUserResolveContributor : IClientResolveContributor
{
    public Task<string> ResolveClientAsync(HttpContext httpContext)
    {
        if (httpContext.User?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult(string.Empty);
        }

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(userId);
        }

        var userName = httpContext.User.Identity?.Name;
        return Task.FromResult(userName ?? string.Empty);
    }
}
