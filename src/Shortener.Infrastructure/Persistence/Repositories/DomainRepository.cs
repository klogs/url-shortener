using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;
using Shortener.Domain.Enums;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class DomainRepository(ShortenerDbContext db) : IDomainRepository
{
    public Task<TenantDomain?> GetByNormalizedHostAsync(string normalizedHost, CancellationToken ct)
        => db.Domains.FirstOrDefaultAsync(d => d.NormalizedHost == normalizedHost, ct);

    public Task<TenantDomain?> GetDefaultForTenantAsync(Guid tenantId, CancellationToken ct)
        => db.Domains.FirstOrDefaultAsync(
            d => d.TenantId == tenantId && d.IsDefault && d.Status == DomainStatus.Active, ct);

    public Task<TenantDomain?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => db.Domains.FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId, ct);

    public Task<int> CountCustomByTenantAsync(Guid tenantId, CancellationToken ct)
        => db.Domains.CountAsync(d => d.TenantId == tenantId && !d.IsDefault && d.Status != DomainStatus.Disabled, ct);

    public async Task<IReadOnlyList<TenantDomain>> ListByTenantAsync(Guid tenantId, CancellationToken ct)
        => await db.Domains
            .Where(d => d.TenantId == tenantId && d.Status != DomainStatus.Disabled)
            .OrderBy(d => d.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task InsertAsync(TenantDomain domain, CancellationToken ct)
    {
        db.Domains.Add(domain);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TenantDomain domain, CancellationToken ct)
    {
        db.Domains.Update(domain);
        await db.SaveChangesAsync(ct);
    }
}
