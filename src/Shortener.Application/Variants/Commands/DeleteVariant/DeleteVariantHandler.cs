using Shortener.Application.Interfaces;

namespace Shortener.Application.Variants.Commands.DeleteVariant;

public sealed class DeleteVariantHandler(
    IShortLinkRepository links,
    ILinkVariantRepository variants)
{
    public async Task HandleAsync(DeleteVariantCommand cmd, CancellationToken ct = default)
    {
        var variant = await variants.GetByIdAsync(cmd.VariantId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Variant not found.");

        await variants.DeleteAsync(variant.Id, ct);

        var remaining = await variants.CountByLinkAsync(variant.LinkId, ct);
        if (remaining == 0)
        {
            var link = await links.GetByIdAsync(variant.LinkId, cmd.TenantId, ct);
            if (link is not null && link.IsAbTest)
            {
                link.DisableAbTest();
                await links.UpdateAsync(link, ct);
            }
        }
    }
}
