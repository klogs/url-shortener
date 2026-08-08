using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class TenantRepository(ShortenerDbContext db) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task InsertAsync(Tenant tenant, CancellationToken ct)
    {
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);
    }
}
