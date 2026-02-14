using Microsoft.AspNetCore.Authorization;

namespace Sistema.ABAC.API.Authorization;

/// <summary>
/// Atributo de autorización ABAC que aplica la política "AbacPolicy".
/// </summary>
public sealed class AbacAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyName = "AbacPolicy";

    public AbacAuthorizeAttribute()
    {
        Policy = PolicyName;
    }
}