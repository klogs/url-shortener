using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;
using Shortener.Domain.Enums;

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

    public async Task<(IReadOnlyList<ShortLink> Items, bool HasMore)> ListAsync(
        Guid tenantId, int pageSize, Guid? afterId, CancellationToken ct)
    {
        var query = db.ShortLinks
            .Where(l => l.TenantId == tenantId && l.Status != LinkStatus.Deleted)
            .OrderByDescending(l => l.CreatedAtUtc)
            .ThenBy(l => l.Id);

        if (afterId.HasValue)
        {
            var cursor = await db.ShortLinks.FindAsync([afterId.Value], ct);
            if (cursor is not null)
            {
                query = (IOrderedQueryable<ShortLink>)query.Where(
                    l => l.CreatedAtUtc < cursor.CreatedAtUtc ||
                         (l.CreatedAtUtc == cursor.CreatedAtUtc && l.Id > cursor.Id));
            }
        }

        var items = await query.Take(pageSize + 1).ToListAsync(ct);
        var hasMore = items.Count > pageSize;
        return (items.Take(pageSize).ToList(), hasMore);
    }

    public Task<int> CountActiveByTenantAsync(Guid tenantId, CancellationToken ct)
        => db.ShortLinks.CountAsync(l => l.TenantId == tenantId && l.Status != LinkStatus.Deleted, ct);

    public async Task<IReadOnlyList<ShortLink>> ListExpiringSoonAsync(
        DateTimeOffset from, DateTimeOffset to, int batchSize, CancellationToken ct)
        => await db.ShortLinks
            .Where(l => l.Status == LinkStatus.Active
                && l.ExpiresAt.HasValue
                && l.ExpiresAt.Value >= from
                && l.ExpiresAt.Value <= to)
            .OrderBy(l => l.ExpiresAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ShortLink>> ListAboveReportThresholdAsync(int threshold, int batchSize, CancellationToken ct)
        => await db.ShortLinks
            .Where(l => l.ReportCount >= threshold && l.Status == LinkStatus.Active)
            .OrderByDescending(l => l.ReportCount)
            .Take(batchSize)
            .ToListAsync(ct);

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

    public async Task DeleteAllByTenantAsync(Guid tenantId, CancellationToken ct)
        => await db.ShortLinks
            .Where(l => l.TenantId == tenantId)
            .ExecuteDeleteAsync(ct);
}
