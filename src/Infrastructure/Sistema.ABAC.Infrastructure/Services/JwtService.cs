using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Infrastructure.Settings;

namespace Sistema.ABAC.Infrastructure.Services;

/// <summary>
/// Implementación del servicio JWT para generación y validación de tokens.
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    /// <inheritdoc />
    public Task<TokenDto> GenerateTokenAsync(User user, IEnumerable<string> roles, IEnumerable<Claim> userClaims)
    {
        // Crear lista de claims para el token
        var claims = new List<Claim>
        {
            // Claims estándar de JWT
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            
            // Claims personalizados del usuario
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("fullName", user.FullName)
        };

        // Agregar roles como claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Agregar claims adicionales del usuario
        foreach (var claim in userClaims)
        {
            claims.Add(claim);
        }

        // Crear la clave de seguridad
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Calcular fecha de expiración
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        // Crear el token JWT
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials
        );

        // Generar el string del token
        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(token);

        // Crear el DTO de respuesta
        var tokenDto = new TokenDto
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationInMinutes * 60, // Convertir a segundos
            ExpiresAt = expiresAt,
            RefreshToken = null, // Por ahora no implementamos refresh tokens
            User = null! // Se asignará en AuthService
        };

        return Task.FromResult(tokenDto);
    }
}
