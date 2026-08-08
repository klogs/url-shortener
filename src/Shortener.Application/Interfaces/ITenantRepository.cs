using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Tenant> Items, bool HasMore)> ListAllAsync(int pageSize, Guid? afterId, CancellationToken ct = default);
    Task<int> CountAllAsync(CancellationToken ct = default);
    Task InsertAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
}
