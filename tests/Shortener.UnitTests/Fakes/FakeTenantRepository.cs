using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.UnitTests.Fakes;

internal sealed class FakeTenantRepository : ITenantRepository
{
    private readonly List<Tenant> _store = [];

    public void Seed(Tenant tenant) => _store.Add(tenant);

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(t => t.Id == id));

    public Task<(IReadOnlyList<Tenant> Items, bool HasMore)> ListAllAsync(int pageSize, Guid? afterId, CancellationToken ct)
    {
        var ordered = _store.OrderBy(t => t.CreatedAtUtc).ThenBy(t => t.Id).AsEnumerable();
        if (afterId.HasValue)
        {
            var cursor = _store.FirstOrDefault(t => t.Id == afterId.Value);
            if (cursor is not null)
            {
                ordered = ordered.Where(t => t.CreatedAtUtc > cursor.CreatedAtUtc ||
                    (t.CreatedAtUtc == cursor.CreatedAtUtc && t.Id > cursor.Id));
            }
        }

        var items = ordered.Take(pageSize + 1).ToList();
        var hasMore = items.Count > pageSize;
        return Task.FromResult<(IReadOnlyList<Tenant> Items, bool HasMore)>((items.Take(pageSize).ToList(), hasMore));
    }

    public Task<int> CountAllAsync(CancellationToken ct)
        => Task.FromResult(_store.Count);

    public Task InsertAsync(Tenant tenant, CancellationToken ct)
    {
        _store.Add(tenant);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken ct) => Task.CompletedTask;
}
