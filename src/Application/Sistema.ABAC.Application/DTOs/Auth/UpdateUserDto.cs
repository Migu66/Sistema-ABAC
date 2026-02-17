using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.Application.DTOs.Auth;

/// <summary>
/// DTO para actualizar información de un usuario.
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    [MaxLength(200, ErrorMessage = "El nombre completo no puede exceder 200 caracteres")]
    public string? FullName { get; set; }

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    [MaxLength(256, ErrorMessage = "El correo electrónico no puede exceder 256 caracteres")]
    public string? Email { get; set; }

    /// <summary>
    /// Número de teléfono del usuario.
    /// </summary>
    [Phone(ErrorMessage = "El número de teléfono no es válido")]
    [MaxLength(20, ErrorMessage = "El número de teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Departamento al que pertenece el usuario.
    /// </summary>
    [MaxLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Department { get; set; }

    /// <summary>
    /// Indica si el usuario está activo en el sistema.
    /// </summary>
    public bool? IsActive { get; set; }
}

