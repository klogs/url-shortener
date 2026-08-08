using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.UnitTests.Fakes;

internal sealed class FakeDomainRepository : IDomainRepository
{
    private TenantDomain? _domain;

    public void SetDomain(TenantDomain domain) => _domain = domain;

    public Task<TenantDomain?> GetByNormalizedHostAsync(string normalizedHost, CancellationToken ct)
        => Task.FromResult(_domain?.NormalizedHost == normalizedHost ? _domain : null);

    public Task<TenantDomain?> GetDefaultForTenantAsync(Guid tenantId, CancellationToken ct)
        => Task.FromResult(_domain?.TenantId == tenantId ? _domain : null);

    public Task<TenantDomain?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => Task.FromResult(_domain?.Id == id && _domain?.TenantId == tenantId ? _domain : null);

    public Task InsertAsync(TenantDomain domain, CancellationToken ct)
    {
        _domain = domain;
        return Task.CompletedTask;
    }
}
