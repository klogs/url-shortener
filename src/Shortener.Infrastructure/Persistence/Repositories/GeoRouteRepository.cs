using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;
using Shortener.Infrastructure.Persistence;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class GeoRouteRepository(ShortenerDbContext db) : IGeoRouteRepository
{
    public async Task<IReadOnlyList<GeoRoute>> ListByLinkAsync(Guid linkId, Guid tenantId, CancellationToken ct = default)
        => await db.GeoRoutes
            .Where(r => r.LinkId == linkId && r.TenantId == tenantId)
            .ToListAsync(ct);

    public async Task<GeoRoute?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await db.GeoRoutes
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, ct);

    public async Task InsertAsync(GeoRoute route, CancellationToken ct = default)
    {
        db.GeoRoutes.Add(route);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await db.GeoRoutes.Where(r => r.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task<int> CountByLinkAsync(Guid linkId, CancellationToken ct = default)
        => await db.GeoRoutes.CountAsync(r => r.LinkId == linkId, ct);
}
