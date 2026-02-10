using Sistema.ABAC.Domain.Common;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa un recurso del sistema sobre el cual se controla el acceso mediante políticas ABAC.
/// Un recurso puede ser cualquier entidad, objeto, archivo, endpoint o dato al que se desea proteger.
/// </summary>
/// <remarks>
/// Los recursos son uno de los tres componentes principales en la evaluación ABAC:
/// - Subject (Usuario): Quién intenta acceder
/// - Resource (Recurso): A qué intenta acceder
/// - Action (Acción): Qué intenta hacer
/// </remarks>
/// <example>
/// Ejemplos de recursos:
/// - Name: "Reporte de Ventas Q4", Type: "documento", Description: "Informe trimestral del área de ventas"
/// - Name: "/api/salaries", Type: "endpoint", Description: "API para consultar información de nóminas"
/// - Name: "Panel de Administración", Type: "vista", Description: "Interfaz de administración del sistema"
/// - Name: "Base de Datos Producción", Type: "database", Description: "BD principal del sistema"
/// </example>
public class Resource : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo del recurso.
    /// </summary>
    /// <example>
    /// "Reporte de Ventas Q4", "Panel de Administración", "Documento Confidencial"
    /// </example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tipo o categoría del recurso.
    /// Permite agrupar y clasificar recursos similares.
    /// </summary>
    /// <example>
    /// "documento", "endpoint", "carpeta", "vista", "database", "archivo"
    /// </example>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del recurso, su propósito y contenido.
    /// </summary>
    /// <example>
    /// "Informe trimestral con datos de ventas. Contiene información confidencial de clientes y proyecciones."
    /// </example>
    public string? Description { get; set; }

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Colección de atributos asignados a este recurso.
    /// Los atributos del recurso se evalúan en las políticas ABAC
    /// (ej: clasificación, propietario, departamento_responsable, nivel_confidencialidad).
    /// </summary>
    public virtual ICollection<ResourceAttribute> ResourceAttributes { get; set; } = new List<ResourceAttribute>();

    /// <summary>
    /// Colección de registros de auditoría de intentos de acceso a este recurso.
    /// </summary>
    public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
}
