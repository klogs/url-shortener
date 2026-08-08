namespace Shortener.Application.Interfaces;

public sealed record CachedRedirect(
    Guid LinkId,
    string DestinationUrl,
    string Status,
    DateTimeOffset? ExpiresAt,
    int RedirectStatusCode,
    bool IsAbTest = false,
    bool HasGeoRoutes = false);

// Lightweight geo route payload stored in Redis.
public sealed record CachedGeoRoute(string CountryCode, string DestinationUrl);

// Lightweight variant payload stored in Redis for A/B selection.
public sealed record CachedVariant(int Weight, string DestinationUrl);

public interface IRedirectCache
{
    Task<CachedRedirect?> GetAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
    Task SetAsync(string normalizedHost, string shortCode, CachedRedirect value, DateTimeOffset? linkExpiresAt, CancellationToken ct = default);
    Task SetNegativeAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
    Task RemoveAsync(string normalizedHost, string shortCode, CancellationToken ct = default);

    Task<IReadOnlyList<CachedVariant>?> GetVariantsAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
    Task SetVariantsAsync(string normalizedHost, string shortCode, IReadOnlyList<CachedVariant> variants, CancellationToken ct = default);
    Task RemoveVariantsAsync(string normalizedHost, string shortCode, CancellationToken ct = default);

    Task<IReadOnlyList<CachedGeoRoute>?> GetGeoRoutesAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
    Task SetGeoRoutesAsync(string normalizedHost, string shortCode, IReadOnlyList<CachedGeoRoute> routes, CancellationToken ct = default);
    Task RemoveGeoRoutesAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
}
