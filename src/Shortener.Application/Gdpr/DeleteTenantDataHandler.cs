using Shortener.Application.Interfaces;

namespace Shortener.Application.Gdpr;

public sealed class DeleteTenantDataHandler(
    IShortLinkRepository links,
    IClickEventRepository clickEvents)
{
    public async Task HandleAsync(DeleteTenantDataCommand command, CancellationToken ct = default)
    {
        // Scrub visitor PII (IP, UA, referer) from all click events for this tenant first,
        // so the data is gone even if the link cascade delete leaves orphaned partitions.
        await clickEvents.AnonymizeByTenantAsync(command.TenantId, ct);

        // Delete all links for the tenant — cascades to variants, geo routes, click events.
        await links.DeleteAllByTenantAsync(command.TenantId, ct);
    }
}
