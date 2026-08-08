using Shortener.Application.Interfaces;

namespace Shortener.Application.Links.Commands.DeleteLink;

public sealed class DeleteLinkHandler(IShortLinkRepository links, TimeProvider time)
{
    public async Task HandleAsync(DeleteLinkCommand cmd, CancellationToken ct = default)
    {
        var link = await links.GetByIdAsync(cmd.Id, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Link not found.");

        link.SoftDelete(cmd.DeletedBy, time.GetUtcNow());
        await links.UpdateAsync(link, ct);
    }
}
