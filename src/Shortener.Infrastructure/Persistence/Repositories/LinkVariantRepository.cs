using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class LinkVariantRepository(ShortenerDbContext db) : ILinkVariantRepository
{
    public async Task<IReadOnlyList<LinkVariant>> ListByLinkAsync(Guid linkId, Guid tenantId, CancellationToken ct)
        => await db.LinkVariants
            .Where(v => v.LinkId == linkId && v.TenantId == tenantId)
            .OrderBy(v => v.CreatedAtUtc)
            .ToListAsync(ct);

    public Task<LinkVariant?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => db.LinkVariants.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId, ct);

    public async Task InsertAsync(LinkVariant variant, CancellationToken ct)
    {
        db.LinkVariants.Add(variant);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await db.LinkVariants.Where(v => v.Id == id).ExecuteDeleteAsync(ct);
    }

    public Task<int> CountByLinkAsync(Guid linkId, CancellationToken ct)
        => db.LinkVariants.CountAsync(v => v.LinkId == linkId, ct);
}
