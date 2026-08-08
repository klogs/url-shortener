namespace Shortener.Application.Interfaces;

/// <summary>
/// Invalidates both the Redis redirect cache and the upstream edge cache for a single link.
/// </summary>
public interface ILinkCacheInvalidator
{
    Task InvalidateAsync(Guid domainId, Guid tenantId, string shortCode, CancellationToken ct = default);
}
