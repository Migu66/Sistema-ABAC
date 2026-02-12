namespace Sistema.ABAC.Application.DTOs.Auth;

/// <summary>
/// DTO con información del usuario autenticado.
/// No incluye datos sensibles como contraseñas.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre de usuario.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Número de teléfono del usuario.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Departamento al que pertenece el usuario.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Indica si el correo electrónico ha sido confirmado.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Indica si el usuario está activo en el sistema.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Roles del usuario en el sistema.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Fecha de creación de la cuenta (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Última fecha de inicio de sesión (UTC).
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
