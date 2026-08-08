using Shortener.Domain.Enums;

namespace Shortener.Domain.Entities;

public sealed class TenantDomain
{
    private TenantDomain() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }

    public string Host { get; private set; } = default!;
    public string NormalizedHost { get; private set; } = default!;

    public DomainStatus Status { get; private set; }
    public bool IsDefault { get; private set; }

    public string? VerificationToken { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static TenantDomain Create(Guid tenantId, string host, bool isDefault, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        return new TenantDomain
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Host = host.Trim(),
            NormalizedHost = NormalizeHost(host),
            Status = DomainStatus.Pending,
            IsDefault = isDefault,
            VerificationToken = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = now
        };
    }

    // Used for self-hosted single-tenant mode where verification is skipped
    public static TenantDomain CreateVerified(Guid tenantId, string host, bool isDefault, DateTimeOffset now)
    {
        var domain = Create(tenantId, host, isDefault, now);
        domain.Status = DomainStatus.Active;
        domain.VerifiedAt = now;
        return domain;
    }

    public void MarkVerified(DateTimeOffset now)
    {
        Status = DomainStatus.Active;
        VerifiedAt = now;
    }

    public void Disable() => Status = DomainStatus.Disabled;

    public static string NormalizeHost(string host)
        => host.Trim().ToLowerInvariant().TrimEnd('/');
}
