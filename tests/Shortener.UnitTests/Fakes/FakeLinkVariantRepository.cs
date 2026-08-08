using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.UnitTests.Fakes;

internal sealed class FakeLinkVariantRepository : ILinkVariantRepository
{
    private readonly List<LinkVariant> _store = [];

    public Task<IReadOnlyList<LinkVariant>> ListByLinkAsync(Guid linkId, Guid tenantId, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<LinkVariant>>(
            _store.Where(v => v.LinkId == linkId && v.TenantId == tenantId).ToList());

    public Task<LinkVariant?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => Task.FromResult(_store.FirstOrDefault(v => v.Id == id && v.TenantId == tenantId));

    public Task InsertAsync(LinkVariant variant, CancellationToken ct)
    {
        _store.Add(variant);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _store.RemoveAll(v => v.Id == id);
        return Task.CompletedTask;
    }

    public Task<int> CountByLinkAsync(Guid linkId, CancellationToken ct)
        => Task.FromResult(_store.Count(v => v.LinkId == linkId));
}
