using Shortener.Application.Interfaces;

namespace Shortener.Application.Links.Commands.BlockLink;

public sealed class BlockLinkHandler(IShortLinkRepository links, TimeProvider time)
{
    public async Task HandleAsync(BlockLinkCommand cmd, CancellationToken ct = default)
    {
        var link = await links.GetByIdAsync(cmd.LinkId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Link not found.");

        link.Block(cmd.BlockedBy, time.GetUtcNow());
        await links.UpdateAsync(link, ct);
    }
}
