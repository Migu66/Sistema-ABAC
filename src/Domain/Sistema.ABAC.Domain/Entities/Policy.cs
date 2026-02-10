using Sistema.ABAC.Domain.Common;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa una política ABAC que define reglas de control de acceso basadas en atributos.
/// Las políticas son el corazón del sistema ABAC y determinan si se permite o deniega el acceso a recursos.
/// </summary>
/// <remarks>
/// Una política ABAC consta de:
/// - Un conjunto de condiciones que deben evaluarse (PolicyConditions)
/// - Un efecto (Permit o Deny) que se aplica si todas las condiciones se cumplen
/// - Una prioridad para resolver conflictos cuando múltiples políticas aplican
/// - Un estado activo/inactivo para habilitar/deshabilitar políticas sin eliminarlas
/// 
/// El motor de evaluación ABAC evalúa todas las políticas activas aplicables y combina sus resultados
/// según estrategias como "Deny-Overrides" (cualquier Deny gana) o "Permit-Overrides" (cualquier Permit gana).
/// </remarks>
/// <example>
/// Ejemplos de políticas:
/// - Name: "Acceso Gerencial", Effect: Permit, Priority: 100
///   Descripción: "Los gerentes pueden leer todos los recursos de su departamento"
///   Condiciones: User.Rol == "Gerente" AND User.Departamento == Resource.Departamento
/// 
/// - Name: "Bloqueo Horario Nocturno", Effect: Deny, Priority: 200
///   Descripción: "Nadie puede acceder a datos sensibles fuera del horario laboral"
///   Condiciones: Resource.Clasificacion == "Sensible" AND Environment.Hora NOT IN [8..18]
/// 
/// - Name: "Acceso Administrador Global", Effect: Permit, Priority: 999
///   Descripción: "Los administradores tienen acceso total a todos los recursos"
///   Condiciones: User.Rol == "Administrador"
/// </example>
public class Policy : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo de la política (para visualización humana e identificación).
    /// </summary>
    /// <example>
    /// "Acceso Gerencial a Reportes", "Restricción Horaria Datos Sensibles", "Permiso Lectura Documentos Públicos"
    /// </example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del propósito de la política y cuándo se aplica.
    /// Ayuda a los administradores a entender la lógica de negocio detrás de la política.
    /// </summary>
    /// <example>
    /// "Permite a los gerentes acceder a todos los reportes de su departamento durante horario laboral.
    /// Esta política garantiza que los supervisores tengan visibilidad completa de su área."
    /// </example>
    public string? Description { get; set; }

    /// <summary>
    /// Efecto que tendrá la política cuando todas sus condiciones se cumplan.
    /// Puede ser Permit (permite el acceso) o Deny (deniega el acceso).
    /// </summary>
    /// <remarks>
    /// En estrategias Deny-Overrides, cualquier política con Effect=Deny que aplique
    /// resultará en denegación de acceso, sin importar cuántas políticas Permit apliquen.
    /// </remarks>
    public PolicyEffect Effect { get; set; }

    /// <summary>
    /// Prioridad numérica de la política para resolver conflictos.
    /// Un valor mayor indica mayor prioridad.
    /// </summary>
    /// <remarks>
    /// La prioridad se usa cuando múltiples políticas con efectos diferentes aplican al mismo contexto.
    /// En empates, generalmente se aplica una estrategia de combinación por defecto (ej: Deny-Overrides).
    /// </remarks>
    /// <example>
    /// - Políticas generales: Priority = 100
    /// - Políticas departamentales: Priority = 200
    /// - Políticas de seguridad críticas: Priority = 500
    /// - Políticas de administrador: Priority = 999
    /// </example>
    public int Priority { get; set; }

    /// <summary>
    /// Indica si la política está activa y debe ser evaluada.
    /// Permite deshabilitar políticas temporalmente sin eliminarlas de la base de datos.
    /// </summary>
    /// <remarks>
    /// Las políticas inactivas (IsActive=false) son ignoradas por el motor de evaluación ABAC.
    /// Esto es útil para:
    /// - Deshabilitar temporalmente una política durante mantenimiento
    /// - Probar el sistema sin ciertas restricciones
    /// - Mantener histórico de políticas que ya no aplican
    /// </remarks>
    public bool IsActive { get; set; } = true;

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Condiciones que deben cumplirse para que esta política aplique.
    /// Todas las condiciones se evalúan con operador lógico AND (todas deben ser verdaderas).
    /// </summary>
    /// <example>
    /// Una política puede tener varias condiciones:
    /// - Condición 1: User.Departamento == "Ventas"
    /// - Condición 2: Resource.Clasificacion IN ["Pública", "Interna"]
    /// - Condición 3: Environment.HoraDia >= 8 AND Environment.HoraDia &lt;= 18
    /// </example>
    public ICollection<PolicyCondition> Conditions { get; set; } = new List<PolicyCondition>();

    /// <summary>
    /// Relación many-to-many con acciones a través de PolicyAction.
    /// Define qué acciones están cubiertas por esta política.
    /// </summary>
    /// <remarks>
    /// Una política puede aplicar a múltiples acciones. Por ejemplo:
    /// - Una política de "Lectura Permitida" aplica solo a la acción "read"
    /// - Una política de "Acceso Completo" aplica a ["read", "write", "delete"]
    /// </remarks>
    public ICollection<PolicyAction> PolicyActions { get; set; } = new List<PolicyAction>();

    // TODO: Paso 19 - Descomentar cuando se cree AccessLog
    // Registros de auditoría que documentan cuándo esta política fue aplicada en decisiones de acceso.
    // Permite rastrear cuántas veces se ha aplicado la política y en qué contextos.
    // public ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
}
