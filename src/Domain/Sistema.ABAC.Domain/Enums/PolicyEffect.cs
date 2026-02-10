namespace Sistema.ABAC.Domain.Enums;

/// <summary>
/// Define el efecto que tendrá una política ABAC cuando sus condiciones se cumplan.
/// </summary>
public enum PolicyEffect
{
    /// <summary>
    /// La política permite el acceso al recurso.
    /// </summary>
    Permit = 0,

    /// <summary>
    /// La política deniega el acceso al recurso.
    /// </summary>
    Deny = 1
}
