using Microsoft.Extensions.Caching.Memory;

namespace Sistema.ABAC.API.Security;

/// <summary>
/// Implementación de blacklist de tokens en memoria.
/// </summary>
public class MemoryTokenBlacklistService : ITokenBlacklistService
{
    private const string KeyPrefix = "token-blacklist:";
    private static readonly TimeSpan MinimumTtl = TimeSpan.FromMinutes(1);
    private readonly IMemoryCache _memoryCache;

    public MemoryTokenBlacklistService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task BlacklistTokenAsync(string tokenId, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return Task.CompletedTask;
        }

        var utcNow = DateTime.UtcNow;
        var ttl = expiresAtUtc <= utcNow
            ? MinimumTtl
            : expiresAtUtc - utcNow;

        _memoryCache.Set(BuildKey(tokenId), true, ttl);
        return Task.CompletedTask;
    }

    public bool IsTokenBlacklisted(string tokenId)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return false;
        }

        return _memoryCache.TryGetValue(BuildKey(tokenId), out _);
    }

    private static string BuildKey(string tokenId) => $"{KeyPrefix}{tokenId}";
}
