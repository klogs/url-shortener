using Shortener.Application.Links.Commands.CreateLink;

namespace Shortener.Application.Links.Commands.BulkCreateLinks;

public sealed class BulkCreateLinksHandler(CreateLinkHandler createLink)
{
    public const int MaxBatchSize = 100;

    public async Task<IReadOnlyList<CreateLinkResult>> HandleAsync(
        BulkCreateLinksCommand cmd, CancellationToken ct = default)
    {
        if (cmd.Links.Count == 0)
        {
            throw new ArgumentException("At least one link is required.");
        }

        if (cmd.Links.Count > MaxBatchSize)
        {
            throw new ArgumentException($"Bulk create is limited to {MaxBatchSize} links per request.");
        }

        var results = new List<CreateLinkResult>(cmd.Links.Count);

        foreach (var item in cmd.Links)
        {
            var command = new CreateLinkCommand(
                cmd.TenantId,
                cmd.DomainId,
                item.DestinationUrl,
                item.Alias,
                item.ExpiresAt,
                IsAnonymous: false,
                CreatedBy: cmd.CreatedBy,
                CaptchaToken: null,
                ClientIp: null);

            var result = await createLink.HandleAsync(command, ct);
            results.Add(result);
        }

        return results;
    }
}
