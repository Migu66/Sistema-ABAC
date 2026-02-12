namespace Sistema.ABAC.Application.DTOs.Auth;

/// <summary>
/// DTO que contiene el token JWT y su información relacionada.
/// </summary>
public class TokenDto
{
    /// <summary>
    /// Token JWT de acceso.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de token (generalmente "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tiempo de expiración del token en segundos.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Fecha y hora de expiración del token (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token de actualización (refresh token) para obtener un nuevo token de acceso.
    /// Opcional, se usa si el sistema implementa refresh tokens.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Información básica del usuario autenticado.
    /// </summary>
    public UserDto User { get; set; } = null!;
}
