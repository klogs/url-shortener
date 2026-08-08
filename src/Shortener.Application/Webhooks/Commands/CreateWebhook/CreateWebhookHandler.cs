using System.Security.Cryptography;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Application.Webhooks.Commands.CreateWebhook;

public sealed class CreateWebhookHandler(IWebhookRepository webhooks, TimeProvider time)
{
    public async Task<CreateWebhookResult> HandleAsync(CreateWebhookCommand cmd, CancellationToken ct = default)
    {
        if (cmd.Events.Count == 0)
        {
            throw new ArgumentException("At least one event type is required.");
        }

        // Generate a cryptographically random HMAC signing secret.
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var webhook = Webhook.Create(cmd.TenantId, cmd.Url, secret, cmd.Events, time.GetUtcNow());
        await webhooks.InsertAsync(webhook, ct);

        return new CreateWebhookResult(
            webhook.Id,
            webhook.Url,
            webhook.Events.Split(',', StringSplitOptions.RemoveEmptyEntries),
            webhook.IsActive,
            webhook.CreatedAtUtc);
    }
}
