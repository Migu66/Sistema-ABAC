namespace Sistema.ABAC.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando un usuario no tiene permisos suficientes para realizar una acción.
/// Se mapea a HTTP 403 Forbidden.
/// </summary>
public class ForbiddenAccessException : ApplicationException
{
    public ForbiddenAccessException()
        : base("No tienes permisos suficientes para realizar esta acción.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
