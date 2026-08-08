using Shortener.Application.Interfaces;
using Shortener.Domain.Enums;

namespace Shortener.Application.Links.Commands.UnblockLink;

public sealed class UnblockLinkHandler(IShortLinkRepository links, ILinkCacheInvalidator cacheInvalidator, TimeProvider time)
{
    public async Task HandleAsync(UnblockLinkCommand cmd, CancellationToken ct = default)
    {
        var link = await links.GetByIdAsync(cmd.LinkId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Link not found.");

        if (link.Status != LinkStatus.Blocked)
        {
            throw new InvalidOperationException("Link is not blocked.");
        }

        link.Unblock(cmd.UnblockedBy, time.GetUtcNow());
        await links.UpdateAsync(link, ct);
        await cacheInvalidator.InvalidateAsync(link.DomainId, link.TenantId, link.ShortCode, ct);
    }
}
