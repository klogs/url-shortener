using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByPrefixAsync(string keyPrefix, CancellationToken ct = default);
    Task<ApiKey?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ApiKey>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task InsertAsync(ApiKey apiKey, CancellationToken ct = default);
    Task UpdateAsync(ApiKey apiKey, CancellationToken ct = default);
}
