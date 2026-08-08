using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.UnitTests.Fakes;

internal sealed class FakeTenantRepository : ITenantRepository
{
    private readonly List<Tenant> _store = [];

    public void Seed(Tenant tenant) => _store.Add(tenant);

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(t => t.Id == id));

    public Task InsertAsync(Tenant tenant, CancellationToken ct)
    {
        _store.Add(tenant);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken ct) => Task.CompletedTask;
}
