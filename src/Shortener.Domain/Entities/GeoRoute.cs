namespace Shortener.Domain.Entities;

public sealed class GeoRoute
{
    private GeoRoute() { }

    public Guid Id { get; private set; }
    public Guid LinkId { get; private set; }
    public Guid TenantId { get; private set; }

    // ISO 3166-1 alpha-2, e.g. "US", "DE", "TR"
    public string CountryCode { get; private set; } = default!;
    public string DestinationUrl { get; private set; } = default!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static GeoRoute Create(
        Guid linkId,
        Guid tenantId,
        string countryCode,
        string destinationUrl,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);

        var code = countryCode.Trim().ToUpperInvariant();
        if (code.Length != 2)
        {
            throw new ArgumentException("Country code must be a 2-letter ISO 3166-1 alpha-2 code.", nameof(countryCode));
        }

        return new GeoRoute
        {
            Id = Guid.NewGuid(),
            LinkId = linkId,
            TenantId = tenantId,
            CountryCode = code,
            DestinationUrl = destinationUrl,
            CreatedAtUtc = now
        };
    }
}
