namespace Sistema.ABAC.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando se intenta crear una entidad que ya existe.
/// Se mapea a HTTP 409 Conflict.
/// </summary>
public class ConflictException : ApplicationException
{
    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string entityName, object key)
        : base($"La entidad '{entityName}' con clave '{key}' ya existe en el sistema.")
    {
    }
}
