using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.UnitTests.Fakes;

internal sealed class FakeGeoRouteRepository : IGeoRouteRepository
{
    private readonly List<GeoRoute> _store = [];

    public Task<IReadOnlyList<GeoRoute>> ListByLinkAsync(Guid linkId, Guid tenantId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<GeoRoute>>(_store.Where(r => r.LinkId == linkId && r.TenantId == tenantId).ToList());

    public Task<GeoRoute?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => Task.FromResult(_store.FirstOrDefault(r => r.Id == id && r.TenantId == tenantId));

    public Task InsertAsync(GeoRoute route, CancellationToken ct = default)
    {
        _store.Add(route);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }

    public Task<int> CountByLinkAsync(Guid linkId, CancellationToken ct = default)
        => Task.FromResult(_store.Count(r => r.LinkId == linkId));
}
