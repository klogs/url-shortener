using Shortener.Application.Interfaces;

namespace Shortener.Application.Webhooks.Commands.UpdateWebhook;

public sealed class UpdateWebhookHandler(IWebhookRepository webhooks)
{
    public async Task HandleAsync(UpdateWebhookCommand cmd, CancellationToken ct = default)
    {
        var webhook = await webhooks.GetByIdAsync(cmd.WebhookId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Webhook not found.");

        webhook.Update(cmd.Url, cmd.Events, cmd.IsActive);
        await webhooks.UpdateAsync(webhook, ct);
    }
}
