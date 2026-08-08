using Shortener.Application.Interfaces;

namespace Shortener.Application.ApiKeys.Queries.ListApiKeys;

public sealed class ListApiKeysHandler(IApiKeyRepository apiKeys)
{
    public async Task<IReadOnlyList<ApiKeyDto>> HandleAsync(ListApiKeysQuery query, CancellationToken ct = default)
    {
        var list = await apiKeys.ListByTenantAsync(query.TenantId, ct);
        return list.Select(k => new ApiKeyDto(
            k.Id,
            k.Name,
            k.KeyPrefix,
            k.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries),
            k.IsRevoked,
            k.ExpiresAt,
            k.LastUsedAtUtc,
            k.CreatedAtUtc)).ToList();
    }
}
