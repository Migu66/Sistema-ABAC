using Sistema.ABAC.Domain.Common;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa la definición de un atributo que puede ser asignado a usuarios, recursos o contextos.
/// Los atributos son las características que se evalúan en las políticas ABAC.
/// </summary>
/// <remarks>
/// Esta entidad define el "esquema" o "plantilla" de un atributo, no el valor en sí.
/// Por ejemplo: "departamento" es un atributo, "Ventas" sería un valor asignado a ese atributo.
/// </remarks>
/// <example>
/// Ejemplos de atributos:
/// - Name: "Departamento", Key: "departamento", Type: String
/// - Name: "Nivel de Acceso", Key: "nivel_acceso", Type: Number
/// - Name: "Es Gerente", Key: "es_gerente", Type: Boolean
/// - Name: "Fecha de Contratación", Key: "fecha_contratacion", Type: DateTime
/// </example>
public class Attribute : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo del atributo (para visualización humana).
    /// </summary>
    /// <example>
    /// "Departamento", "Nivel de Acceso", "Clasificación de Seguridad"
    /// </example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Clave técnica del atributo (usada en evaluación de políticas).
    /// Debe ser única, sin espacios, preferiblemente en snake_case o camelCase.
    /// </summary>
    /// <example>
    /// "departamento", "nivel_acceso", "clasificacion_seguridad"
    /// </example>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de dato que almacena este atributo.
    /// Determina cómo se validará y comparará el valor en las políticas.
    /// </summary>
    public AttributeType Type { get; set; }

    /// <summary>
    /// Descripción detallada del propósito y uso del atributo.
    /// </summary>
    /// <example>
    /// "Departamento organizacional al que pertenece el usuario. Se usa para controlar acceso a recursos departamentales."
    /// </example>
    public string? Description { get; set; }

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Colección de valores asignados a usuarios para este atributo.
    /// </summary>
    public virtual ICollection<UserAttribute> UserAttributes { get; set; } = new List<UserAttribute>();

    /// <summary>
    /// Colección de valores asignados a recursos para este atributo.
    /// </summary>
    public virtual ICollection<ResourceAttribute> ResourceAttributes { get; set; } = new List<ResourceAttribute>();
}
