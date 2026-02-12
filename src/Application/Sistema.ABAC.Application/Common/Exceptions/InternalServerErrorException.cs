namespace Sistema.ABAC.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando ocurre un error inesperado en el servidor.
/// Se mapea a HTTP 500 Internal Server Error.
/// </summary>
public class InternalServerErrorException : ApplicationException
{
    public InternalServerErrorException(string message)
        : base(message)
    {
    }

    public InternalServerErrorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
