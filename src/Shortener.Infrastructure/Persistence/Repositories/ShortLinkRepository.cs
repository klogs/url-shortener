using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class ShortLinkRepository(ShortenerDbContext db) : IShortLinkRepository
{
    public Task<ShortLink?> GetByHostAndCodeAsync(string normalizedHost, string shortCode, CancellationToken ct)
        => db.ShortLinks
            .Join(db.Domains,
                l => l.DomainId,
                d => d.Id,
                (l, d) => new { Link = l, Domain = d })
            .Where(x => x.Domain.NormalizedHost == normalizedHost && x.Link.ShortCode == shortCode)
            .Select(x => x.Link)
            .FirstOrDefaultAsync(ct);

    public Task<ShortLink?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => db.ShortLinks.FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId, ct);

    public Task<bool> AliasExistsAsync(Guid domainId, string shortCode, CancellationToken ct)
        => db.ShortLinks.AnyAsync(l => l.DomainId == domainId && l.ShortCode == shortCode, ct);

    public async Task InsertAsync(ShortLink link, CancellationToken ct)
    {
        db.ShortLinks.Add(link);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ShortLink link, CancellationToken ct)
    {
        db.ShortLinks.Update(link);
        await db.SaveChangesAsync(ct);
    }
}
