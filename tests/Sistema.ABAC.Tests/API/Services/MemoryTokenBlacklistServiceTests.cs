using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Sistema.ABAC.API.Security;

namespace Sistema.ABAC.Tests.API.Services;

public class MemoryTokenBlacklistServiceTests
{
    private readonly IMemoryCache _cache;
    private readonly MemoryTokenBlacklistService _sut;

    public MemoryTokenBlacklistServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _sut = new MemoryTokenBlacklistService(_cache);
    }

    [Fact]
    public async Task BlacklistTokenAsync_WithValidToken_MakesItBlacklisted()
    {
        var tokenId = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        await _sut.BlacklistTokenAsync(tokenId, expiresAt);

        _sut.IsTokenBlacklisted(tokenId).Should().BeTrue();
    }

    [Fact]
    public async Task BlacklistTokenAsync_WithNullTokenId_DoesNotThrow()
    {
        await _sut.BlacklistTokenAsync(null!, DateTime.UtcNow.AddHours(1));
        // no exception
    }

    [Fact]
    public async Task BlacklistTokenAsync_WithEmptyTokenId_DoesNotThrow()
    {
        await _sut.BlacklistTokenAsync("", DateTime.UtcNow.AddHours(1));
        // no exception
    }

    [Fact]
    public async Task BlacklistTokenAsync_WithWhitespaceTokenId_DoesNotThrow()
    {
        await _sut.BlacklistTokenAsync("   ", DateTime.UtcNow.AddHours(1));
        // whitespace tokens are ignored
    }

    [Fact]
    public async Task BlacklistTokenAsync_WithExpiredToken_UsesMinimumTtl()
    {
        var tokenId = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(-10); // already expired

        await _sut.BlacklistTokenAsync(tokenId, expiresAt);

        // Debería estar en caché por al menos MinimumTtl (1 min)
        _sut.IsTokenBlacklisted(tokenId).Should().BeTrue();
    }

    [Fact]
    public void IsTokenBlacklisted_WhenNotBlacklisted_ReturnsFalse()
    {
        _sut.IsTokenBlacklisted("nonexistent-token").Should().BeFalse();
    }

    [Fact]
    public void IsTokenBlacklisted_WithNullTokenId_ReturnsFalse()
    {
        _sut.IsTokenBlacklisted(null!).Should().BeFalse();
    }

    [Fact]
    public void IsTokenBlacklisted_WithEmptyTokenId_ReturnsFalse()
    {
        _sut.IsTokenBlacklisted("").Should().BeFalse();
    }

    [Fact]
    public void IsTokenBlacklisted_WithWhitespace_ReturnsFalse()
    {
        _sut.IsTokenBlacklisted("   ").Should().BeFalse();
    }
}
