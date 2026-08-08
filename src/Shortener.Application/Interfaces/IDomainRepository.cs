using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface IDomainRepository
{
    Task<TenantDomain?> GetByNormalizedHostAsync(string normalizedHost, CancellationToken ct = default);
    Task<TenantDomain?> GetDefaultForTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantDomain?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<TenantDomain>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task InsertAsync(TenantDomain domain, CancellationToken ct = default);
    Task UpdateAsync(TenantDomain domain, CancellationToken ct = default);
}
