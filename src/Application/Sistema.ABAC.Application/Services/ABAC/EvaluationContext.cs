namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Contexto de evaluación ABAC que encapsula atributos del sujeto, recurso, acción y entorno.
/// </summary>
public class EvaluationContext
{
    /// <summary>
    /// Atributos del sujeto (usuario) que solicita acceso.
    /// </summary>
    public IDictionary<string, object?> Subject { get; }

    /// <summary>
    /// Atributos del recurso sobre el cual se solicita acceso.
    /// </summary>
    public IDictionary<string, object?> Resource { get; }

    /// <summary>
    /// Atributos de la acción solicitada.
    /// </summary>
    public IDictionary<string, object?> Action { get; }

    /// <summary>
    /// Atributos del entorno/contexto de la solicitud (hora, IP, ubicación, etc.).
    /// </summary>
    public IDictionary<string, object?> Environment { get; }

    /// <summary>
    /// Crea una nueva instancia de <see cref="EvaluationContext"/>.
    /// </summary>
    /// <param name="subject">Atributos del sujeto</param>
    /// <param name="resource">Atributos del recurso</param>
    /// <param name="action">Atributos de la acción</param>
    /// <param name="environment">Atributos del entorno</param>
    public EvaluationContext(
        IDictionary<string, object?>? subject = null,
        IDictionary<string, object?>? resource = null,
        IDictionary<string, object?>? action = null,
        IDictionary<string, object?>? environment = null)
    {
        Subject = CreateDictionary(subject);
        Resource = CreateDictionary(resource);
        Action = CreateDictionary(action);
        Environment = CreateDictionary(environment);
    }

    private static IDictionary<string, object?> CreateDictionary(IDictionary<string, object?>? source)
    {
        return source == null
            ? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object?>(source, StringComparer.OrdinalIgnoreCase);
    }
}