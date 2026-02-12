using Sistema.ABAC.Application.DTOs.Auth;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz del servicio de autenticación.
/// Define las operaciones básicas de autenticación y autorización del sistema.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="registerDto">Datos del usuario a registrar.</param>
    /// <returns>Token de acceso JWT y datos del usuario registrado.</returns>
    Task<TokenDto> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Autentica un usuario existente en el sistema.
    /// </summary>
    /// <param name="loginDto">Credenciales de inicio de sesión.</param>
    /// <returns>Token de acceso JWT y datos del usuario autenticado.</returns>
    Task<TokenDto> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// Genera un nuevo token JWT para un usuario específico.
    /// </summary>
    /// <param name="userId">ID del usuario para el cual generar el token.</param>
    /// <returns>Token de acceso JWT con la información del usuario.</returns>
    Task<TokenDto> GenerateTokenAsync(Guid userId);
}
