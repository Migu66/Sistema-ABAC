using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs.Auth;

/// <summary>
/// DTO para autenticación de usuarios (inicio de sesión).
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Nombre de usuario o correo electrónico.
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Indica si se debe recordar la sesión (token de mayor duración).
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
