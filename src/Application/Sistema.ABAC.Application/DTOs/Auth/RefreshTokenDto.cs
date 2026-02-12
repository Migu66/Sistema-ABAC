using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs.Auth;

/// <summary>
/// DTO para renovar un token JWT expirado usando un refresh token.
/// </summary>
public class RefreshTokenDto
{
    /// <summary>
    /// Token JWT expirado o próximo a expirar.
    /// </summary>
    [Required(ErrorMessage = "El token de acceso es requerido")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token de actualización válido.
    /// </summary>
    [Required(ErrorMessage = "El token de actualización es requerido")]
    public string RefreshToken { get; set; } = string.Empty;
}
