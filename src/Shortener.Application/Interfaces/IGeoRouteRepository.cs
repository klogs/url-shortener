using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface IGeoRouteRepository
{
    Task<IReadOnlyList<GeoRoute>> ListByLinkAsync(Guid linkId, Guid tenantId, CancellationToken ct = default);
    Task<GeoRoute?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task InsertAsync(GeoRoute route, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
