using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Application.Variants.Commands.CreateVariant;

public sealed class CreateVariantHandler(
    IShortLinkRepository links,
    ILinkVariantRepository variants,
    TimeProvider time)
{
    public async Task<CreateVariantResult> HandleAsync(CreateVariantCommand cmd, CancellationToken ct = default)
    {
        var link = await links.GetByIdAsync(cmd.LinkId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Link not found.");

        var variant = LinkVariant.Create(
            cmd.LinkId, cmd.TenantId, cmd.Label, cmd.DestinationUrl, cmd.Weight, time.GetUtcNow());

        await variants.InsertAsync(variant, ct);

        if (!link.IsAbTest)
        {
            link.EnableAbTest();
            await links.UpdateAsync(link, ct);
        }

        return new CreateVariantResult(
            variant.Id, variant.LinkId, variant.Label,
            variant.DestinationUrl, variant.Weight, variant.CreatedAtUtc);
    }
}
