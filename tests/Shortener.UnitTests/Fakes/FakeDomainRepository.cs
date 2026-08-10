using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.UnitTests.Fakes;

internal sealed class FakeDomainRepository : IDomainRepository
{
    private readonly List<TenantDomain> _store = [];

    public void Seed(TenantDomain domain) => _store.Add(domain);

    public Task<TenantDomain?> GetByNormalizedHostAsync(string normalizedHost, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(d => d.NormalizedHost == normalizedHost));

    public Task<TenantDomain?> GetDefaultForTenantAsync(Guid tenantId, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(d => d.TenantId == tenantId && d.IsDefault));

    public Task<TenantDomain?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(d => d.Id == id && d.TenantId == tenantId));

    public Task<int> CountCustomByTenantAsync(Guid tenantId, CancellationToken ct)
        => Task.FromResult(_store.Count(d => d.TenantId == tenantId && !d.IsDefault));

    public Task<IReadOnlyList<TenantDomain>> ListByTenantAsync(Guid tenantId, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<TenantDomain>>(_store.Where(d => d.TenantId == tenantId).ToList());

    public Task InsertAsync(TenantDomain domain, CancellationToken ct)
    {
        _store.Add(domain);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TenantDomain domain, CancellationToken ct) => Task.CompletedTask;

    public Task<TenantDomain?> GetActiveByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(d => d.Id == id && d.TenantId == tenantId));

    public Task SetDefaultAsync(Guid domainId, Guid tenantId, CancellationToken ct)
    {
        foreach (var d in _store.Where(d => d.TenantId == tenantId)) { d.UnsetDefault(); }
        var target = _store.FirstOrDefault(d => d.Id == domainId && d.TenantId == tenantId);
        target?.SetAsDefault();
        return Task.CompletedTask;
    }
}
