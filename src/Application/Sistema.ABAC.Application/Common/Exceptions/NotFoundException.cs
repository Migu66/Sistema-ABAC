namespace Sistema.ABAC.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando un recurso solicitado no se encuentra en el sistema.
/// Se mapea a HTTP 404 Not Found.
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object key)
        : base($"La entidad '{entityName}' con clave '{key}' no fue encontrada.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
}
