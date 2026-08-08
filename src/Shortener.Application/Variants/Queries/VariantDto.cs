namespace Shortener.Application.Variants.Queries;

public sealed record VariantDto(
    Guid Id,
    Guid LinkId,
    string Label,
    string DestinationUrl,
    int Weight,
    DateTimeOffset CreatedAtUtc);
