using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class TenantRepository(ShortenerDbContext db) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<(IReadOnlyList<Tenant> Items, bool HasMore)> ListAllAsync(int pageSize, Guid? afterId, CancellationToken ct)
    {
        var query = db.Tenants.OrderBy(t => t.CreatedAtUtc).ThenBy(t => t.Id).AsQueryable();
        if (afterId.HasValue)
        {
            var cursor = await db.Tenants.FindAsync([afterId.Value], ct);
            if (cursor is not null)
            {
                query = query.Where(t => t.CreatedAtUtc > cursor.CreatedAtUtc ||
                    (t.CreatedAtUtc == cursor.CreatedAtUtc && t.Id > cursor.Id));
            }
        }

        var items = await query.Take(pageSize + 1).ToListAsync(ct);
        var hasMore = items.Count > pageSize;
        return (items.Take(pageSize).ToList(), hasMore);
    }

    public Task<int> CountAllAsync(CancellationToken ct)
        => db.Tenants.CountAsync(ct);

    public async Task InsertAsync(Tenant tenant, CancellationToken ct)
    {
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct)
    {
        db.Tenants.Update(tenant);
        await db.SaveChangesAsync(ct);
    }
}
