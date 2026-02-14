namespace Sistema.ABAC.Application.Services.ABAC;

/// <summary>
/// Define el contrato para recopilar atributos necesarios durante la evaluación ABAC.
/// </summary>
public interface IAttributeCollectorService
{
    /// <summary>
    /// Recopila los atributos del sujeto (usuario).
    /// </summary>
    /// <param name="userId">ID del usuario a evaluar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Diccionario de atributos del sujeto, indexado por clave</returns>
    Task<IDictionary<string, object?>> CollectSubjectAttributesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recopila los atributos del recurso solicitado.
    /// </summary>
    /// <param name="resourceId">ID del recurso a evaluar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Diccionario de atributos del recurso, indexado por clave</returns>
    Task<IDictionary<string, object?>> CollectResourceAttributesAsync(
        Guid resourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recopila atributos del entorno para la evaluación (hora, IP, día de semana, etc.).
    /// </summary>
    /// <param name="contextAttributes">Atributos de contexto proporcionados por el consumidor</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Diccionario de atributos del entorno, indexado por clave</returns>
    Task<IDictionary<string, object?>> CollectEnvironmentAttributesAsync(
        IDictionary<string, object?>? contextAttributes = null,
        CancellationToken cancellationToken = default);
}