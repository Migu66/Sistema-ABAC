namespace Sistema.ABAC.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando la solicitud contiene datos inválidos o mal formados.
/// Se mapea a HTTP 400 Bad Request.
/// </summary>
public class BadRequestException : ApplicationException
{
    public BadRequestException(string message)
        : base(message)
    {
    }

    public BadRequestException(string parameterName, string reason)
        : base($"El parámetro '{parameterName}' es inválido: {reason}")
    {
    }
}
