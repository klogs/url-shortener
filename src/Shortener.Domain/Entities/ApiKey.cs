namespace Shortener.Domain.Entities;

public sealed class ApiKey
{
    private ApiKey() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;

    // First 8 chars of the raw key stored plaintext — used to look up the hash quickly.
    public string KeyPrefix { get; private set; } = default!;
    // SHA-256 of the full raw key — never store the raw key.
    public string KeyHash { get; private set; } = default!;

    // Space-separated scope tokens e.g. "links:read links:write".
    public string Scopes { get; private set; } = default!;

    public DateTimeOffset? ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? LastUsedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static ApiKey Create(
        Guid tenantId,
        string name,
        string keyPrefix,
        string keyHash,
        string scopes,
        DateTimeOffset? expiresAt,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopes);

        return new ApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            KeyPrefix = keyPrefix,
            KeyHash = keyHash,
            Scopes = scopes,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            CreatedAtUtc = now,
        };
    }

    public void Revoke() => IsRevoked = true;

    public void RecordUsage(DateTimeOffset now) => LastUsedAtUtc = now;

    public bool IsExpired(DateTimeOffset now) => ExpiresAt.HasValue && ExpiresAt.Value <= now;

    public bool HasScope(string scope)
        => Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(s => s.Equals(scope, StringComparison.OrdinalIgnoreCase));
}
