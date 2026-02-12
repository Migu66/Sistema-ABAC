namespace Sistema.ABAC.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando un usuario intenta acceder sin autenticarse.
/// Se mapea a HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException()
        : base("No estás autenticado. Por favor, inicia sesión para acceder a este recurso.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
