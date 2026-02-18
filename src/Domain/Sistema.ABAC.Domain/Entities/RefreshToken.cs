namespace Sistema.ABAC.Domain.Entities;

/// <summary>
/// Representa un token de actualización (refresh token) para renovar tokens JWT expirados.
/// </summary>
/// <remarks>
/// Los refresh tokens permiten a los usuarios obtener nuevos access tokens sin tener que 
/// volver a autenticarse. Se almacenan en la base de datos para validación y revocación.
/// </remarks>
public class RefreshToken
{
    /// <summary>
    /// Identificador único del refresh token.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Token de actualización (cadena única generada aleatoriamente).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario propietario de este refresh token.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Fecha y hora de creación del token.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora de expiración del token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indica si el token ha sido revocado manualmente.
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Fecha y hora en que se revocó el token (si aplica).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Dirección IP desde la cual se creó el token.
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// ID del token que reemplazó a este (cuando se usa para renovación).
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Indica si el token está activo (no expirado y no revocado).
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    // ============================================================
    // RELACIONES DE NAVEGACIÓN
    // ============================================================

    /// <summary>
    /// Usuario propietario de este refresh token.
    /// </summary>
    public virtual User User { get; set; } = null!;
}
