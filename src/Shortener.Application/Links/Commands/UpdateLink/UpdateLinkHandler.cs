using Shortener.Application.Interfaces;

namespace Shortener.Application.Links.Commands.UpdateLink;

public sealed class UpdateLinkHandler(IShortLinkRepository links, TimeProvider time)
{
    public async Task HandleAsync(UpdateLinkCommand cmd, CancellationToken ct = default)
    {
        var link = await links.GetByIdAsync(cmd.Id, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Link not found.");

        if (!Uri.TryCreate(cmd.DestinationUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Destination URL must be a valid http or https URL.");
        }

        var now = time.GetUtcNow();
        link.UpdateDestination(cmd.DestinationUrl, cmd.UpdatedBy, now);

        if (cmd.Title is not null)
        {
            link.UpdateTitle(cmd.Title, cmd.UpdatedBy, now);
        }

        if (cmd.ExpiresAt.HasValue)
        {
            link.UpdateExpiry(cmd.ExpiresAt.Value, cmd.UpdatedBy, now);
        }

        link.UpdateRedirectType(cmd.RedirectType, cmd.UpdatedBy, now);

        await links.UpdateAsync(link, ct);
    }
}
