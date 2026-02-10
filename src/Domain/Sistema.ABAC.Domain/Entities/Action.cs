using Sistema.ABAC.Domain.Common;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa una acción que puede ser realizada sobre un recurso en el sistema ABAC.
/// Las acciones son el tercer componente en la evaluación de políticas (Subject, Resource, Action).
/// </summary>
/// <remarks>
/// Las acciones definen QUÉ puede hacer un usuario con un recurso.
/// Por ejemplo: leer, escribir, eliminar, aprobar, exportar, etc.
/// Las acciones se asocian tanto a políticas (que permiten/deniegan la acción)
/// como a registros de auditoría (qué acción intentó realizar el usuario).
/// </remarks>
/// <example>
/// Ejemplos de acciones:
/// - Name: "Leer", Code: "read", Description: "Permite visualizar el contenido del recurso"
/// - Name: "Eliminar", Code: "delete", Description: "Permite borrar el recurso del sistema"
/// - Name: "Aprobar", Code: "approve", Description: "Permite aprobar una solicitud o documento"
/// - Name: "Exportar", Code: "export", Description: "Permite exportar datos a formatos externos"
/// </example>
public class Action : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo de la acción (para visualización humana).
    /// </summary>
    /// <example>
    /// "Leer", "Eliminar", "Aprobar Documento", "Generar Reporte"
    /// </example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Código único de la acción (usado en evaluación de políticas y código).
    /// Debe ser único, sin espacios, preferiblemente en minúsculas y en inglés.
    /// </summary>
    /// <example>
    /// "read", "delete", "approve", "export", "generate_report"
    /// </example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de qué permite hacer esta acción.
    /// </summary>
    /// <example>
    /// "Permite al usuario visualizar el contenido completo del recurso sin posibilidad de modificación."
    /// </example>
    public string? Description { get; set; }

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Colección de políticas que incluyen esta acción.
    /// Una política puede aplicarse a múltiples acciones (lectura y escritura, por ejemplo).
    /// </summary>
    public virtual ICollection<PolicyAction> PolicyActions { get; set; } = new List<PolicyAction>();

    /// <summary>
    /// Colección de registros de auditoría que registran intentos de realizar esta acción.
    /// </summary>
    public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
}
