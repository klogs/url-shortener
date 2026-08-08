namespace Shortener.Application.Interfaces;

/// <summary>
/// Purges an entry from an upstream CDN or edge cache.
/// Default implementation is a no-op; replace with Cloudflare/Fastly adapter as needed.
/// </summary>
public interface IEdgeCacheInvalidator
{
    Task InvalidateAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
}
