using Sistema.ABAC.Domain.Common;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa un registro de auditoría de una decisión de control de acceso en el sistema ABAC.
/// Cada evaluación de política genera un AccessLog que documenta quién intentó hacer qué, cuándo, y cuál fue el resultado.
/// </summary>
/// <remarks>
/// Los AccessLog son fundamentales para:
/// - Auditoría de seguridad: Rastrear todos los intentos de acceso (exitosos y fallidos)
/// - Análisis forense: Investigar incidentes de seguridad
/// - Cumplimiento normativo: Demostrar conformidad con regulaciones (GDPR, HIPAA, SOX, etc.)
/// - Métricas y análisis: Identificar patrones de uso y posibles anomalías
/// - Debugging: Entender por qué una política permitió o denegó acceso
/// 
/// Cada log captura el contexto completo de la solicitud de acceso:
/// - Quién (UserId): El usuario que intenta acceder
/// - Qué (ResourceId, ActionId): El recurso y la acción solicitada
/// - Cuándo (Timestamp): Momento exacto de la evaluación
/// - Dónde (IpAddress, Context): Ubicación y contexto de la solicitud
/// - Resultado (Result): Si fue permitido, denegado o error
/// - Por qué (PolicyId, Reason): Qué política se aplicó y la razón de la decisión
/// </remarks>
/// <example>
/// Ejemplos de registros de auditoría:
/// 
/// 1. Acceso Permitido:
///    - UserId: "user-123"
///    - ResourceId: "documento-financiero-456"
///    - ActionId: "read"
///    - Result: "Permit"
///    - PolicyId: "policy-gerente-finanzas"
///    - Reason: "Usuario es gerente del departamento de finanzas"
///    - IpAddress: "192.168.1.100"
///    - Timestamp: 2026-02-10 14:30:00
/// 
/// 2. Acceso Denegado:
///    - UserId: "user-789"
///    - ResourceId: "datos-nomina"
///    - ActionId: "export"
///    - Result: "Deny"
///    - PolicyId: "policy-restriccion-horaria"
///    - Reason: "Acceso fuera del horario laboral (20:00)"
///    - IpAddress: "192.168.1.200"
///    - Timestamp: 2026-02-10 20:00:00
/// 
/// 3. Error de Evaluación:
///    - UserId: "user-456"
///    - ResourceId: "recurso-inexistente"
///    - ActionId: "read"
///    - Result: "Error"
///    - Reason: "Recurso no encontrado en el sistema"
///    - IpAddress: "192.168.1.150"
///    - Timestamp: 2026-02-10 15:45:00
/// </example>
public class AccessLog : BaseEntity
{
    /// <summary>
    /// Identificador del usuario que intentó acceder al recurso.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identificador del recurso al que se intentó acceder.
    /// Puede ser null si la evaluación no está asociada a un recurso específico.
    /// </summary>
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// Identificador de la acción que se intentó realizar.
    /// Puede ser null si la evaluación no está asociada a una acción específica.
    /// </summary>
    public Guid? ActionId { get; set; }

    /// <summary>
    /// Identificador de la política que produjo la decisión final.
    /// Puede ser null si ninguna política aplicó o si hubo un error.
    /// </summary>
    /// <remarks>
    /// En sistemas con múltiples políticas aplicables, este es el ID de la política
    /// que tuvo el efecto determinante (generalmente la de mayor prioridad o la primera Deny).
    /// </remarks>
    public Guid? PolicyId { get; set; }

    /// <summary>
    /// Resultado de la evaluación de acceso.
    /// Valores típicos: "Permit", "Deny", "Error", "NotApplicable"
    /// </summary>
    /// <example>
    /// "Permit" - Acceso permitido por las políticas
    /// "Deny" - Acceso denegado por las políticas
    /// "Error" - Error durante la evaluación
    /// "NotApplicable" - No hay políticas aplicables
    /// </example>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Razón detallada de la decisión de acceso.
    /// Explica por qué se permitió o denegó el acceso.
    /// </summary>
    /// <example>
    /// "Acceso permitido: Usuario pertenece al departamento de Finanzas y tiene nivel >= 5"
    /// "Acceso denegado: Fuera del horario laboral permitido (8:00-18:00)"
    /// "Acceso denegado: Usuario no tiene el atributo 'certificacion_requerida'"
    /// </example>
    public string? Reason { get; set; }

    /// <summary>
    /// Información contextual adicional de la solicitud en formato JSON.
    /// Puede incluir cualquier dato relevante para auditoría.
    /// </summary>
    /// <remarks>
    /// Ejemplos de información contextual:
    /// - User-Agent del navegador
    /// - URL completa de la solicitud
    /// - Headers HTTP relevantes
    /// - Atributos de entorno evaluados (hora, día de semana, ubicación)
    /// - Duración de la evaluación
    /// - Todas las políticas consideradas
    /// </remarks>
    /// <example>
    /// {
    ///   "userAgent": "Mozilla/5.0...",
    ///   "requestUrl": "/api/resources/123",
    ///   "evaluationTimeMs": 45,
    ///   "environment": {
    ///     "hour": 14,
    ///     "dayOfWeek": "Monday",
    ///     "isBusinessHours": true
    ///   },
    ///   "policiesEvaluated": ["policy-1", "policy-2"]
    /// }
    /// </example>
    public string? Context { get; set; }

    /// <summary>
    /// Dirección IP desde la cual se realizó la solicitud.
    /// Útil para detectar accesos desde ubicaciones inusuales o para geolocalización.
    /// </summary>
    /// <example>
    /// "192.168.1.100", "10.0.0.50", "203.0.113.42"
    /// </example>
    public string? IpAddress { get; set; }

    // NOTA: Timestamp ya está disponible como CreatedAt en BaseEntity

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Usuario que realizó el intento de acceso.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Recurso al que se intentó acceder.
    /// Puede ser null si no está asociado a un recurso específico.
    /// </summary>
    public virtual Resource? Resource { get; set; }

    /// <summary>
    /// Acción que se intentó realizar.
    /// Puede ser null si no está asociada a una acción específica.
    /// </summary>
    public virtual Action? Action { get; set; }

    /// <summary>
    /// Política que produjo la decisión final.
    /// Puede ser null si ninguna política aplicó o si hubo un error.
    /// </summary>
    public virtual Policy? Policy { get; set; }
}
