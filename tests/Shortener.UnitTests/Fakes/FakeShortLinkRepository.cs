using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.UnitTests.Fakes;

internal sealed class FakeShortLinkRepository : IShortLinkRepository
{
    private readonly List<ShortLink> _store = [];

    public Task<ShortLink?> GetByHostAndCodeAsync(string normalizedHost, string shortCode, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(l => l.ShortCode == shortCode));

    public Task<ShortLink?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(l => l.Id == id && l.TenantId == tenantId));

    public Task<bool> AliasExistsAsync(Guid domainId, string shortCode, CancellationToken ct)
        => Task.FromResult(_store.Any(l => l.DomainId == domainId && l.ShortCode == shortCode));

    public Task<(IReadOnlyList<ShortLink> Items, bool HasMore)> ListAsync(
        Guid tenantId, int pageSize, Guid? afterId, CancellationToken ct)
    {
        var items = _store
            .Where(l => l.TenantId == tenantId)
            .OrderByDescending(l => l.CreatedAtUtc)
            .Take(pageSize + 1)
            .ToList();
        var hasMore = items.Count > pageSize;
        return Task.FromResult(((IReadOnlyList<ShortLink>)items.Take(pageSize).ToList(), hasMore));
    }

    public Task<IReadOnlyList<ShortLink>> ListAboveReportThresholdAsync(int threshold, int batchSize, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<ShortLink>>(_store
            .Where(l => l.ReportCount >= threshold)
            .Take(batchSize).ToList());

    public Task InsertAsync(ShortLink link, CancellationToken ct)
    {
        _store.Add(link);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ShortLink link, CancellationToken ct)
        => Task.CompletedTask;
}
