namespace Sistema.ABAC.Domain.Enums;

/// <summary>
/// Define los operadores de comparación que se pueden utilizar en las condiciones
/// de las políticas ABAC para evaluar atributos.
/// </summary>
public enum OperatorType
{
    /// <summary>
    /// Operador de igualdad (==).
    /// Verifica si dos valores son exactamente iguales.
    /// Ejemplo: edad == 18
    /// </summary>
    Equals = 0,

    /// <summary>
    /// Operador de desigualdad (!=).
    /// Verifica si dos valores son diferentes.
    /// Ejemplo: estado != "bloqueado"
    /// </summary>
    NotEquals = 1,

    /// <summary>
    /// Operador mayor que (>).
    /// Verifica si el valor izquierdo es mayor que el derecho.
    /// Ejemplo: nivel > 5
    /// </summary>
    GreaterThan = 2,

    /// <summary>
    /// Operador menor que (<).
    /// Verifica si el valor izquierdo es menor que el derecho.
    /// Ejemplo: edad < 18
    /// </summary>
    LessThan = 3,

    /// <summary>
    /// Operador de contención (Contains).
    /// Verifica si una cadena de texto contiene otra cadena.
    /// Ejemplo: departamento Contains "Admin"
    /// </summary>
    Contains = 4,

    /// <summary>
    /// Operador de pertenencia a lista (In).
    /// Verifica si un valor está presente en una lista de valores permitidos.
    /// Ejemplo: rol In ["admin", "superadmin", "moderador"]
    /// </summary>
    In = 5,

    /// <summary>
    /// Operador de no pertenencia a lista (NotIn).
    /// Verifica si un valor NO está presente en una lista de valores.
    /// Ejemplo: país NotIn ["Irán", "Corea del Norte"]
    /// </summary>
    NotIn = 6
}
