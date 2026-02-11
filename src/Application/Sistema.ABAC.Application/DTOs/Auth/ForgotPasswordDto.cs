using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs.Auth;

/// <summary>
/// DTO para solicitud de recuperación de contraseña.
/// </summary>
public class ForgotPasswordDto
{
    /// <summary>
    /// Correo electrónico del usuario que olvidó su contraseña.
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public string Email { get; set; } = string.Empty;
}
