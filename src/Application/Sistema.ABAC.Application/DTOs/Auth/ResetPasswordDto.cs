using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs.Auth;

/// <summary>
/// DTO para restablecer la contraseña usando un token de recuperación.
/// </summary>
public class ResetPasswordDto
{
    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Token de recuperación enviado al correo del usuario.
    /// </summary>
    [Required(ErrorMessage = "El token de recuperación es requerido")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Nueva contraseña del usuario.
    /// </summary>
    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_])[A-Za-z\d@$!%*?&_]{8,}$",
        ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmación de la nueva contraseña.
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
