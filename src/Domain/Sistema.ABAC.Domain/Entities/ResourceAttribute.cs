using Sistema.ABAC.Domain.Common;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa la asignación de un atributo específico a un recurso con su valor correspondiente.
/// Esta es la tabla que almacena los valores concretos de los atributos para cada recurso.
/// </summary>
/// <remarks>
/// Esta entidad establece la relación many-to-many entre Resource y Attribute,
/// pero además almacena el valor específico que tiene ese recurso para ese atributo.
/// Los atributos del recurso se evalúan en las políticas ABAC junto con los atributos
/// del usuario para determinar si se permite o deniega el acceso.
/// </remarks>
/// <example>
/// Ejemplos de asignaciones:
/// - Recurso: "Reporte Financiero" (ResourceId: xxx), Atributo: "clasificacion" (AttributeId: yyy), Value: "confidencial"
/// - Recurso: "Panel Admin" (ResourceId: zzz), Atributo: "nivel_requerido" (AttributeId: www), Value: "5"
/// - Recurso: "Documento Legal" (ResourceId: aaa), Atributo: "departamento_propietario" (AttributeId: bbb), Value: "Legal"
/// </example>
public class ResourceAttribute : BaseEntity
{
    /// <summary>
    /// Identificador del recurso al que se le asigna este atributo.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Identificador del atributo que se está asignando.
    /// </summary>
    public Guid AttributeId { get; set; }

    /// <summary>
    /// Valor concreto del atributo para este recurso.
    /// Se almacena como string y se convierte al tipo apropiado durante la evaluación
    /// según el tipo definido en el Attribute (String, Number, Boolean, DateTime).
    /// </summary>
    /// <example>
    /// "confidencial", "5", "true", "Finanzas"
    /// </example>
    public string Value { get; set; } = string.Empty;

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Recurso al que pertenece este atributo.
    /// </summary>
    public virtual Resource Resource { get; set; } = null!;

    /// <summary>
    /// Definición del atributo (esquema/plantilla).
    /// Contiene el nombre, clave, tipo y descripción del atributo.
    /// </summary>
    public virtual Attribute Attribute { get; set; } = null!;
}
