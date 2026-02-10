namespace Sistema.ABAC.Domain.Enums;

/// <summary>
/// Define los tipos de datos que puede tener un atributo en el sistema ABAC.
/// Estos tipos se utilizan para validar y procesar correctamente los valores de atributos
/// de usuarios, recursos y contexto.
/// </summary>
public enum AttributeType
{
    /// <summary>
    /// Representa un valor de texto (cadena de caracteres).
    /// Ejemplo: "admin", "Juan Pérez", "departamento-ventas"
    /// </summary>
    String = 0,

    /// <summary>
    /// Representa un valor numérico (entero o decimal).
    /// Ejemplo: 25, 100, 3.14, -50
    /// </summary>
    Number = 1,

    /// <summary>
    /// Representa un valor booleano (verdadero o falso).
    /// Ejemplo: true, false
    /// </summary>
    Boolean = 2,

    /// <summary>
    /// Representa una fecha y hora.
    /// Ejemplo: 2026-02-10T15:30:00, 1990-05-15
    /// </summary>
    DateTime = 3
}
