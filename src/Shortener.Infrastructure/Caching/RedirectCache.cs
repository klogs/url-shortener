using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;

namespace Shortener.Infrastructure.Caching;

internal sealed class RedirectCache(IConnectionMultiplexer redis, IOptions<RedisOptions> options) : IRedirectCache
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly RedisOptions _opts = options.Value;

    // Key format: sl:{normalizedHost}:{shortCode}
    private static string CacheKey(string normalizedHost, string shortCode)
        => $"sl:{normalizedHost}:{shortCode}";

    // Variant cache key: sl:ab:{normalizedHost}:{shortCode}
    private static string VariantKey(string normalizedHost, string shortCode)
        => $"sl:ab:{normalizedHost}:{shortCode}";

    // Negative cache sentinel: empty DestinationUrl
    private const string NegativeSentinel = "";

    public async Task<CachedRedirect?> GetAsync(string normalizedHost, string shortCode, CancellationToken ct)
    {
        var key = CacheKey(normalizedHost, shortCode);
        var entries = await _db.HashGetAllAsync(key);
        if (entries.Length == 0)
        {
            return null;
        }

        var map = entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());

        // Negative cache entry
        if (!map.TryGetValue("dest", out var dest) || dest == NegativeSentinel)
        {
            return new CachedRedirect(Guid.Empty, NegativeSentinel, "NotFound", null, 0);
        }

        var linkId = map.TryGetValue("id", out var idStr) && Guid.TryParse(idStr, out var id) ? id : Guid.Empty;
        var status = map.TryGetValue("status", out var s) ? s : "Active";
        var redirectCode = map.TryGetValue("rtype", out var rt) && int.TryParse(rt, out var rc) ? rc : 302;
        var isAbTest = map.TryGetValue("ab", out var abStr) && abStr == "1";
        DateTimeOffset? expiresAt = null;
        if (map.TryGetValue("exp", out var expStr) && DateTimeOffset.TryParse(expStr, out var exp))
        {
            expiresAt = exp;
        }

        return new CachedRedirect(linkId, dest, status, expiresAt, redirectCode, isAbTest);
    }

    public async Task SetAsync(string normalizedHost, string shortCode, CachedRedirect value,
        DateTimeOffset? linkExpiresAt, CancellationToken ct)
    {
        var key = CacheKey(normalizedHost, shortCode);
        var ttl = ComputeTtl(linkExpiresAt, _opts.RedirectCacheTtlSeconds);

        HashEntry[] entries =
        [
            new("id", value.LinkId.ToString()),
            new("dest", value.DestinationUrl),
            new("status", value.Status),
            new("rtype", value.RedirectStatusCode.ToString()),
            new("ab", value.IsAbTest ? "1" : "0"),
        ];

        if (value.ExpiresAt.HasValue)
        {
            var withExp = new HashEntry[entries.Length + 1];
            entries.CopyTo(withExp, 0);
            withExp[entries.Length] = new HashEntry("exp", value.ExpiresAt.Value.ToString("O"));
            entries = withExp;
        }

        await _db.HashSetAsync(key, entries);
        await _db.KeyExpireAsync(key, ttl);
    }

    public async Task SetNegativeAsync(string normalizedHost, string shortCode, CancellationToken ct)
    {
        var key = CacheKey(normalizedHost, shortCode);
        await _db.HashSetAsync(key, [new HashEntry("dest", NegativeSentinel)]);
        await _db.KeyExpireAsync(key, TimeSpan.FromSeconds(_opts.NegativeCacheTtlSeconds));
    }

    public Task RemoveAsync(string normalizedHost, string shortCode, CancellationToken ct)
        => _db.KeyDeleteAsync(CacheKey(normalizedHost, shortCode));

    public async Task<IReadOnlyList<CachedVariant>?> GetVariantsAsync(
        string normalizedHost, string shortCode, CancellationToken ct)
    {
        var json = await _db.StringGetAsync(VariantKey(normalizedHost, shortCode));
        if (json.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<CachedVariant>>(json.ToString());
    }

    public async Task SetVariantsAsync(
        string normalizedHost, string shortCode, IReadOnlyList<CachedVariant> variants, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(variants);
        await _db.StringSetAsync(
            VariantKey(normalizedHost, shortCode),
            json,
            TimeSpan.FromSeconds(_opts.RedirectCacheTtlSeconds));
    }

    public Task RemoveVariantsAsync(string normalizedHost, string shortCode, CancellationToken ct)
        => _db.KeyDeleteAsync(VariantKey(normalizedHost, shortCode));

    private static TimeSpan ComputeTtl(DateTimeOffset? linkExpiresAt, int defaultTtlSeconds)
    {
        var defaultTtl = TimeSpan.FromSeconds(defaultTtlSeconds);
        if (!linkExpiresAt.HasValue)
        {
            return defaultTtl;
        }

        var untilExpiry = linkExpiresAt.Value - DateTimeOffset.UtcNow;
        return untilExpiry > TimeSpan.Zero
            ? (untilExpiry < defaultTtl ? untilExpiry : defaultTtl)
            : TimeSpan.FromSeconds(1); // already expired — very short cache to drain naturally
    }
}
