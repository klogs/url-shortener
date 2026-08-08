namespace Shortener.Domain.Entities;

public sealed class LinkVariant
{
    private LinkVariant() { }

    public Guid Id { get; private set; }
    public Guid LinkId { get; private set; }
    public Guid TenantId { get; private set; }

    public string Label { get; private set; } = default!;
    public string DestinationUrl { get; private set; } = default!;

    // Relative weight for weighted-random selection (>= 1).
    public int Weight { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static LinkVariant Create(
        Guid linkId,
        Guid tenantId,
        string label,
        string destinationUrl,
        int weight,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);

        if (weight < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1.");
        }

        return new LinkVariant
        {
            Id = Guid.NewGuid(),
            LinkId = linkId,
            TenantId = tenantId,
            Label = label.Trim(),
            DestinationUrl = destinationUrl,
            Weight = weight,
            CreatedAtUtc = now
        };
    }
}
