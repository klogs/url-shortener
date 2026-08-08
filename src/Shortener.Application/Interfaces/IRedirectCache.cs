namespace Shortener.Application.Interfaces;

public sealed record CachedRedirect(
    Guid LinkId,
    string DestinationUrl,
    string Status,
    DateTimeOffset? ExpiresAt,
    int RedirectStatusCode);

public interface IRedirectCache
{
    Task<CachedRedirect?> GetAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
    Task SetAsync(string normalizedHost, string shortCode, CachedRedirect value, DateTimeOffset? linkExpiresAt, CancellationToken ct = default);
    Task SetNegativeAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
    Task RemoveAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
}
