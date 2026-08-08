using Shortener.Application.Interfaces;

namespace Shortener.Infrastructure.Caching;

internal sealed class NoOpEdgeCacheInvalidator : IEdgeCacheInvalidator
{
    public Task InvalidateAsync(string normalizedHost, string shortCode, CancellationToken ct = default)
        => Task.CompletedTask;
}
