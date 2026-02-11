using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs.Auth;

/// <summary>
/// DTO para registro de nuevos usuarios en el sistema.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Nombre de usuario único para el sistema.
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres")]
    [MaxLength(50, ErrorMessage = "El nombre de usuario no puede tener más de 50 caracteres")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmación de la contraseña (debe coincidir con Password).
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre completo no puede tener más de 100 caracteres")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Número de teléfono del usuario (opcional).
    /// </summary>
    [Phone(ErrorMessage = "El número de teléfono no es válido")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Departamento al que pertenece el usuario (opcional).
    /// </summary>
    [MaxLength(100, ErrorMessage = "El departamento no puede tener más de 100 caracteres")]
    public string? Department { get; set; }
}
