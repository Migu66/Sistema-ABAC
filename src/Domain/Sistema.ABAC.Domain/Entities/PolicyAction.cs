using Sistema.ABAC.Domain.Common;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa la asociación entre una política ABAC y las acciones a las que aplica.
/// Esta es una tabla intermedia (many-to-many) entre Policy y Action.
/// </summary>
/// <remarks>
/// Una política puede aplicar a múltiples acciones, y una acción puede estar cubierta
/// por múltiples políticas. Esta entidad establece esa relación.
/// 
/// Por ejemplo:
/// - Una política de "Lectura Permitida para Empleados" aplicaría solo a la acción "read"
/// - Una política de "Acceso Administrativo Completo" aplicaría a ["read", "write", "delete", "approve"]
/// - Una política de "Bloqueo Nocturno" podría aplicar a todas las acciones excepto "read"
/// 
/// Durante la evaluación ABAC, solo se consideran las políticas que están asociadas
/// con la acción específica que el usuario intenta realizar.
/// </remarks>
/// <example>
/// Ejemplos de asociaciones:
/// 
/// Política: "Acceso de Lectura General" (PolicyId: xxx)
/// - PolicyAction 1: ActionId: "read" (yyy)
///   → Esta política solo aplica cuando el usuario intenta leer
/// 
/// Política: "Permisos de Gerente" (PolicyId: zzz)
/// - PolicyAction 1: ActionId: "read" (aaa)
/// - PolicyAction 2: ActionId: "write" (bbb)
/// - PolicyAction 3: ActionId: "approve" (ccc)
///   → Esta política aplica cuando el usuario intenta leer, escribir o aprobar
/// 
/// Política: "Restricción Horaria" (PolicyId: www)
/// - PolicyAction 1: ActionId: "delete" (ddd)
/// - PolicyAction 2: ActionId: "export" (eee)
///   → Esta política solo aplica cuando el usuario intenta eliminar o exportar
/// </example>
public class PolicyAction : BaseEntity
{
    /// <summary>
    /// Identificador de la política que se asocia a una acción.
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Identificador de la acción a la que aplica la política.
    /// </summary>
    public Guid ActionId { get; set; }

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Política asociada que define las condiciones y el efecto (Permit/Deny).
    /// </summary>
    public virtual Policy Policy { get; set; } = null!;

    /// <summary>
    /// Acción a la que aplica esta política.
    /// Define QUÉ operación está siendo controlada por la política.
    /// </summary>
    public virtual Action Action { get; set; } = null!;
}
