namespace Sistema.ABAC.API.Security;

/// <summary>
/// Servicio para gestionar tokens revocados (blacklist).
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Marca un token como revocado hasta su fecha de expiración.
    /// </summary>
    /// <param name="tokenId">Identificador único del token (jti).</param>
    /// <param name="expiresAtUtc">Fecha de expiración del token en UTC.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task BlacklistTokenAsync(string tokenId, DateTime expiresAtUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica si el token está revocado.
    /// </summary>
    /// <param name="tokenId">Identificador único del token (jti).</param>
    /// <returns>True si el token fue revocado; en caso contrario false.</returns>
    bool IsTokenBlacklisted(string tokenId);
}
