using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface IShortLinkRepository
{
    Task<ShortLink?> GetByHostAndCodeAsync(string normalizedHost, string shortCode, CancellationToken ct = default);
    Task<ShortLink?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<bool> AliasExistsAsync(Guid domainId, string shortCode, CancellationToken ct = default);
    Task InsertAsync(ShortLink link, CancellationToken ct = default);
    Task UpdateAsync(ShortLink link, CancellationToken ct = default);
}
