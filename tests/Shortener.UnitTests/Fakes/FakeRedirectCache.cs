using Shortener.Application.Interfaces;

namespace Shortener.UnitTests.Fakes;

internal sealed class FakeRedirectCache : IRedirectCache
{
    private readonly Dictionary<string, CachedRedirect?> _store = [];
    private readonly Dictionary<string, IReadOnlyList<CachedVariant>> _variants = [];

    public void Seed(string host, string code, CachedRedirect? value)
        => _store[$"{host}:{code}"] = value;

    public Task<CachedRedirect?> GetAsync(string normalizedHost, string shortCode, CancellationToken ct)
    {
        _store.TryGetValue($"{normalizedHost}:{shortCode}", out var result);
        return Task.FromResult(result);
    }

    public Task SetAsync(string normalizedHost, string shortCode, CachedRedirect value,
        DateTimeOffset? linkExpiresAt, CancellationToken ct)
    {
        _store[$"{normalizedHost}:{shortCode}"] = value;
        return Task.CompletedTask;
    }

    public Task SetNegativeAsync(string normalizedHost, string shortCode, CancellationToken ct)
    {
        _store[$"{normalizedHost}:{shortCode}"] = new CachedRedirect(Guid.Empty, string.Empty, "NotFound", null, 0);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string normalizedHost, string shortCode, CancellationToken ct)
    {
        _store.Remove($"{normalizedHost}:{shortCode}");
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CachedVariant>?> GetVariantsAsync(string normalizedHost, string shortCode, CancellationToken ct)
    {
        _variants.TryGetValue($"{normalizedHost}:{shortCode}", out var result);
        return Task.FromResult<IReadOnlyList<CachedVariant>?>(result);
    }

    public Task SetVariantsAsync(string normalizedHost, string shortCode, IReadOnlyList<CachedVariant> variants, CancellationToken ct)
    {
        _variants[$"{normalizedHost}:{shortCode}"] = variants;
        return Task.CompletedTask;
    }

    public Task RemoveVariantsAsync(string normalizedHost, string shortCode, CancellationToken ct)
    {
        _variants.Remove($"{normalizedHost}:{shortCode}");
        return Task.CompletedTask;
    }
}
