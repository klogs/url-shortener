namespace Shortener.Application.Variants.Commands.CreateVariant;

public sealed record CreateVariantCommand(
    Guid LinkId,
    Guid TenantId,
    string Label,
    string DestinationUrl,
    int Weight);
