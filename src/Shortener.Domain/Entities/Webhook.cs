namespace Shortener.Domain.Entities;

public sealed class Webhook
{
    private Webhook() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Url { get; private set; } = default!;
    // HMAC-SHA256 signing secret sent as X-Webhook-Signature header.
    public string Secret { get; private set; } = default!;
    // Comma-separated event types, e.g. "link.created,link.deleted".
    public string Events { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Webhook Create(Guid tenantId, string url, string secret, IEnumerable<string> events, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Webhook URL must be a valid http or https URL.");
        }

        return new Webhook
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Url = url,
            Secret = secret,
            Events = string.Join(",", events),
            IsActive = true,
            CreatedAtUtc = now,
        };
    }

    public void Update(string url, IEnumerable<string> events, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Webhook URL must be a valid http or https URL.");
        }

        Url = url;
        Events = string.Join(",", events);
        IsActive = isActive;
    }

    public void Deactivate() => IsActive = false;

    public bool SubscribesTo(string eventType)
        => Events.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(e => e.Trim().Equals(eventType, StringComparison.OrdinalIgnoreCase));
}
