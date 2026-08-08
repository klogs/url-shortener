using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface ILinkVariantRepository
{
    Task<IReadOnlyList<LinkVariant>> ListByLinkAsync(Guid linkId, Guid tenantId, CancellationToken ct = default);
    Task<LinkVariant?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task InsertAsync(LinkVariant variant, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> CountByLinkAsync(Guid linkId, CancellationToken ct = default);
}
