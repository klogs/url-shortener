using Shortener.Domain.Enums;

namespace Shortener.Domain.Entities;

public sealed class ShortLink
{
    private ShortLink() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid DomainId { get; private set; }

    public string ShortCode { get; private set; } = default!;
    public string DestinationUrl { get; private set; } = default!;

    public string? Title { get; private set; }
    public string? Description { get; private set; }

    public LinkStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? LastAccessedAt { get; private set; }

    public RedirectType RedirectType { get; private set; }
    public bool IsAnonymous { get; private set; }

    public long ClickCountSnapshot { get; private set; }
    public int ReportCount { get; private set; }

    // Optimistic concurrency token
    public uint Version { get; private set; }

    public static ShortLink CreateAnonymous(
        Guid tenantId,
        Guid domainId,
        string shortCode,
        string destinationUrl,
        DateTimeOffset expiresAt,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);

        return new ShortLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DomainId = domainId,
            ShortCode = shortCode,
            DestinationUrl = destinationUrl,
            Status = LinkStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            ExpiresAt = expiresAt,
            RedirectType = RedirectType.Found,
            IsAnonymous = true
        };
    }

    public static ShortLink CreateAuthenticated(
        Guid tenantId,
        Guid domainId,
        string shortCode,
        string destinationUrl,
        Guid createdBy,
        DateTimeOffset? expiresAt,
        RedirectType redirectType,
        DateTimeOffset now,
        string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);

        return new ShortLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DomainId = domainId,
            ShortCode = shortCode,
            DestinationUrl = destinationUrl,
            Title = title,
            Status = LinkStatus.Active,
            CreatedAtUtc = now,
            CreatedBy = createdBy,
            UpdatedAtUtc = now,
            ExpiresAt = expiresAt,
            RedirectType = redirectType,
            IsAnonymous = false
        };
    }

    public bool IsExpired(DateTimeOffset now)
        => ExpiresAt.HasValue && ExpiresAt.Value <= now;

    public bool IsRedirectable(DateTimeOffset now)
        => Status == LinkStatus.Active && !IsExpired(now);

    public void Disable(Guid updatedBy, DateTimeOffset now)
    {
        Status = LinkStatus.Disabled;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public void MarkExpired(DateTimeOffset now)
    {
        Status = LinkStatus.Expired;
        UpdatedAtUtc = now;
    }

    public void SoftDelete(Guid updatedBy, DateTimeOffset now)
    {
        Status = LinkStatus.Deleted;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public void RecordAccess(DateTimeOffset now)
        => LastAccessedAt = now;

    public void UpdateDestination(string destinationUrl, Guid updatedBy, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);
        DestinationUrl = destinationUrl;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public void UpdateTitle(string? title, Guid updatedBy, DateTimeOffset now)
    {
        Title = title;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public void UpdateExpiry(DateTimeOffset? expiresAt, Guid updatedBy, DateTimeOffset now)
    {
        ExpiresAt = expiresAt;
        // Re-activate if previously marked Expired and new expiry is in the future
        if (Status == LinkStatus.Expired && (expiresAt is null || expiresAt.Value > now))
        {
            Status = LinkStatus.Active;
        }

        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public void UpdateRedirectType(RedirectType redirectType, Guid updatedBy, DateTimeOffset now)
    {
        RedirectType = redirectType;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public void Block(Guid updatedBy, DateTimeOffset now)
    {
        Status = LinkStatus.Blocked;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public void Unblock(Guid updatedBy, DateTimeOffset now)
    {
        Status = LinkStatus.Active;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public void IncrementClickSnapshot() => ClickCountSnapshot++;

    public void IncrementReportCount() => ReportCount++;

    // Called by sweep worker when report count crosses threshold.
    public void AutoBlock(DateTimeOffset now)
    {
        Status = LinkStatus.Blocked;
        UpdatedAtUtc = now;
    }
}
