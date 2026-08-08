namespace Shortener.Application.Variants.Commands.CreateVariant;

public sealed record CreateVariantResult(
    Guid Id,
    Guid LinkId,
    string Label,
    string DestinationUrl,
    int Weight,
    DateTimeOffset CreatedAtUtc);
