using System.Security.Claims;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz del servicio JWT para generación y validación de tokens.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Genera un token JWT para un usuario específico con sus roles y claims.
    /// </summary>
    /// <param name="user">Usuario para el cual generar el token.</param>
    /// <param name="roles">Roles del usuario.</param>
    /// <param name="claims">Claims adicionales del usuario.</param>
    /// <returns>Token de acceso con información de expiración.</returns>
    Task<TokenDto> GenerateTokenAsync(User user, IEnumerable<string> roles, IEnumerable<Claim> claims);
}
