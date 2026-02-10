using Sistema.ABAC.Domain.Common;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa la asignación de un atributo específico a un usuario con su valor correspondiente.
/// Esta es la tabla que almacena los valores concretos de los atributos para cada usuario.
/// </summary>
/// <remarks>
/// Esta entidad establece la relación many-to-many entre User y Attribute,
/// pero además almacena el valor específico que tiene ese usuario para ese atributo
/// y el período de validez de dicho valor.
/// </remarks>
/// <example>
/// Ejemplos de asignaciones:
/// - Usuario: Juan (UserId: xxx), Atributo: "departamento" (AttributeId: yyy), Value: "Ventas"
/// - Usuario: María (UserId: zzz), Atributo: "nivel_acceso" (AttributeId: www), Value: "5"
/// - Usuario: Pedro (UserId: aaa), Atributo: "es_gerente" (AttributeId: bbb), Value: "true", ValidFrom: 2026-01-01, ValidTo: 2026-12-31
/// </example>
public class UserAttribute : BaseEntity
{
    /// <summary>
    /// Identificador del usuario al que se le asigna este atributo.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identificador del atributo que se está asignando.
    /// </summary>
    public Guid AttributeId { get; set; }

    /// <summary>
    /// Valor concreto del atributo para este usuario.
    /// Se almacena como string y se convierte al tipo apropiado durante la evaluación
    /// según el tipo definido en el Attribute (String, Number, Boolean, DateTime).
    /// </summary>
    /// <example>
    /// "Ventas", "5", "true", "2026-01-15"
    /// </example>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora desde la cual este valor de atributo es válido (inclusiva).
    /// Si es null, el atributo es válido desde siempre.
    /// </summary>
    /// <remarks>
    /// Útil para atributos temporales como promociones, permisos temporales,
    /// roles con fecha de inicio, etc.
    /// </remarks>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// Fecha y hora hasta la cual este valor de atributo es válido (inclusiva).
    /// Si es null, el atributo es válido indefinidamente.
    /// </summary>
    /// <remarks>
    /// Útil para controlar expiración de permisos, accesos temporales,
    /// roles con fecha de fin, etc.
    /// </remarks>
    public DateTime? ValidTo { get; set; }

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Usuario al que pertenece este atributo.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Definición del atributo (esquema/plantilla).
    /// Contiene el nombre, clave, tipo y descripción del atributo.
    /// </summary>
    public virtual Attribute Attribute { get; set; } = null!;
}
