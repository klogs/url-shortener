namespace Shortener.Application.Variants.Commands.DeleteVariant;

public sealed record DeleteVariantCommand(Guid VariantId, Guid TenantId);
