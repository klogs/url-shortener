using System.Security.Cryptography;
using System.Text;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.ApiKeys;

internal sealed class ApiKeyAuthenticator(IApiKeyRepository apiKeys, TimeProvider time) : IApiKeyAuthenticator
{
    private const int PrefixLength = 8;

    public async Task<ApiKey?> AuthenticateAsync(string rawKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawKey) || rawKey.Length < PrefixLength)
        {
            return null;
        }

        var prefix = rawKey[..PrefixLength];
        var apiKey = await apiKeys.GetByPrefixAsync(prefix, ct);

        if (apiKey is null || apiKey.IsRevoked || apiKey.IsExpired(time.GetUtcNow()))
        {
            return null;
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
        if (!hash.Equals(apiKey.KeyHash, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return apiKey;
    }
}
