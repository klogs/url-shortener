using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface IShortLinkRepository
{
    Task<ShortLink?> GetByHostAndCodeAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
    Task<ShortLink?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<bool> AliasExistsAsync(Guid domainId, string shortCode, CancellationToken ct = default);
    Task<(IReadOnlyList<ShortLink> Items, bool HasMore)> ListAsync(
        Guid tenantId, int pageSize, Guid? afterId, CancellationToken ct = default);
    Task<int> CountActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ShortLink>> ListAboveReportThresholdAsync(int threshold, int batchSize, CancellationToken ct = default);
    Task InsertAsync(ShortLink link, CancellationToken ct = default);
    Task UpdateAsync(ShortLink link, CancellationToken ct = default);
    Task DeleteAllByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
