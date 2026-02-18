using Microsoft.AspNetCore.Identity;

namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa un usuario del sistema con capacidades de autenticación proporcionadas por Identity
/// y propiedades adicionales para el sistema ABAC.
/// </summary>
/// <remarks>
/// Hereda de IdentityUser&lt;Guid&gt; para obtener todas las funcionalidades de ASP.NET Core Identity
/// (autenticación, roles, claims, etc.) usando Guid como tipo de clave primaria.
/// </remarks>
public class User : IdentityUser<Guid>
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario (solo lectura, calculado).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Fecha y hora en que se creó el usuario en el sistema.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora de la última modificación del usuario.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indica si el usuario ha sido eliminado lógicamente (soft delete).
    /// Los usuarios eliminados no aparecen en consultas normales pero se conservan en la BD.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    // ============================================================
    // RELACIONES DE NAVEGACIÓN PARA ABAC
    // ============================================================

    /// <summary>
    /// Colección de atributos asignados a este usuario.
    /// Los atributos son características que se evalúan en las políticas ABAC
    /// (ej: departamento, nivel, ubicación, etc.).
    /// </summary>
    public virtual ICollection<UserAttribute> UserAttributes { get; set; } = new List<UserAttribute>();

    /// <summary>
    /// Colección de registros de auditoría de acceso realizados por este usuario.
    /// </summary>
    public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();

    /// <summary>
    /// Colección de tokens de actualización (refresh tokens) asociados a este usuario.
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
