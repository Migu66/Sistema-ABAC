namespace Sistema.ABAC.Application.Common.Exceptions;

/// <summary>
/// Excepción base para todas las excepciones personalizadas del dominio.
/// </summary>
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message)
    {
    }

    protected ApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
