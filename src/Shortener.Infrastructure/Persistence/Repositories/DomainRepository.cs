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

    public async Task InsertAsync(TenantDomain domain, CancellationToken ct)
    {
        db.Domains.Add(domain);
        await db.SaveChangesAsync(ct);
    }
}
