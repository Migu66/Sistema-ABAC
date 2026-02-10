using Sistema.ABAC.Domain.Common;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa una condición individual dentro de una política ABAC.
/// Las condiciones definen los criterios específicos que deben cumplirse para que una política aplique.
/// </summary>
/// <remarks>
/// Una condición compara un atributo (del usuario, recurso o entorno) contra un valor esperado
/// utilizando un operador de comparación. Todas las condiciones de una política se evalúan con
/// lógica AND, es decir, TODAS deben cumplirse para que la política aplique.
/// 
/// Estructura de una condición:
/// [TipoAtributo].[ClaveAtributo] [Operador] [ValorEsperado]
/// 
/// Por ejemplo:
/// - Subject.departamento == "Ventas"
/// - Resource.clasificacion In ["Pública", "Interna"]
/// - Environment.hora >= 8
/// </remarks>
/// <example>
/// Ejemplos de condiciones:
/// 
/// 1. Verificar departamento del usuario:
///    - AttributeType: "Subject"
///    - AttributeKey: "departamento"
///    - Operator: Equals
///    - ExpectedValue: "Ventas"
///    Evaluación: ¿El usuario pertenece al departamento de Ventas?
/// 
/// 2. Verificar clasificación del recurso:
///    - AttributeType: "Resource"
///    - AttributeKey: "clasificacion"
///    - Operator: In
///    - ExpectedValue: "Pública,Interna"
///    Evaluación: ¿El recurso está clasificado como Público o Interno?
/// 
/// 3. Verificar horario de acceso:
///    - AttributeType: "Environment"
///    - AttributeKey: "hora"
///    - Operator: GreaterThan
///    - ExpectedValue: "8"
///    Evaluación: ¿La solicitud se realiza después de las 8am?
/// 
/// 4. Verificar nivel de acceso:
///    - AttributeType: "Subject"
///    - AttributeKey: "nivel_acceso"
///    - Operator: GreaterThan
///    - ExpectedValue: "3"
///    Evaluación: ¿El usuario tiene nivel de acceso mayor a 3?
/// </example>
public class PolicyCondition : BaseEntity
{
    /// <summary>
    /// Identificador de la política a la que pertenece esta condición.
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Tipo de atributo que se evaluará en esta condición.
    /// Indica el origen del atributo: Subject (usuario), Resource (recurso) o Environment (contexto).
    /// </summary>
    /// <remarks>
    /// - Subject: Atributos del usuario que intenta acceder (departamento, rol, nivel, etc.)
    /// - Resource: Atributos del recurso al que se intenta acceder (clasificación, propietario, etc.)
    /// - Environment: Atributos del contexto de la solicitud (hora, fecha, IP, ubicación, etc.)
    /// </remarks>
    /// <example>
    /// "Subject", "Resource", "Environment"
    /// </example>
    public string AttributeType { get; set; } = string.Empty;

    /// <summary>
    /// Clave del atributo que se evaluará.
    /// Debe corresponder a una clave válida en el sistema de atributos.
    /// </summary>
    /// <example>
    /// "departamento", "nivel_acceso", "clasificacion", "hora", "dia_semana"
    /// </example>
    public string AttributeKey { get; set; } = string.Empty;

    /// <summary>
    /// Operador de comparación que se aplicará entre el valor del atributo y el valor esperado.
    /// </summary>
    /// <remarks>
    /// Operadores disponibles:
    /// - Equals (==): Verifica igualdad exacta
    /// - NotEquals (!=): Verifica desigualdad
    /// - GreaterThan (&gt;): Mayor que (para números y fechas)
    /// - LessThan (&lt;): Menor que (para números y fechas)
    /// - Contains: El atributo contiene el valor esperado (para strings)
    /// - In: El atributo está en la lista de valores esperados
    /// - NotIn: El atributo NO está en la lista de valores esperados
    /// </remarks>
    public OperatorType Operator { get; set; }

    /// <summary>
    /// Valor esperado contra el cual se comparará el atributo.
    /// Se almacena como string y se convierte al tipo apropiado durante la evaluación.
    /// </summary>
    /// <remarks>
    /// Para operadores In y NotIn, los valores múltiples se separan por comas.
    /// Ejemplo: "Pública,Interna,Restringida"
    /// 
    /// Para otros operadores, es un valor único.
    /// Ejemplo: "Ventas", "5", "true", "2026-01-15", "08:00"
    /// </remarks>
    /// <example>
    /// - Para Equals: "Ventas"
    /// - Para GreaterThan: "18"
    /// - Para In: "admin,superadmin,moderador"
    /// - Para Contains: "Admin"
    /// </example>
    public string ExpectedValue { get; set; } = string.Empty;

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Política a la que pertenece esta condición.
    /// </summary>
    public virtual Policy Policy { get; set; } = null!;
}
