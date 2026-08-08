using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class ApiKeyRepository(ShortenerDbContext db) : IApiKeyRepository
{
    public Task<ApiKey?> GetByPrefixAsync(string keyPrefix, CancellationToken ct)
        => db.ApiKeys.FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix, ct);

    public Task<ApiKey?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id && k.TenantId == tenantId, ct);

    public async Task<IReadOnlyList<ApiKey>> ListByTenantAsync(Guid tenantId, CancellationToken ct)
        => await db.ApiKeys
            .Where(k => k.TenantId == tenantId)
            .OrderBy(k => k.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task InsertAsync(ApiKey apiKey, CancellationToken ct)
    {
        db.ApiKeys.Add(apiKey);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ApiKey apiKey, CancellationToken ct)
    {
        db.ApiKeys.Update(apiKey);
        await db.SaveChangesAsync(ct);
    }
}
