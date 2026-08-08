using Shortener.Application.Interfaces;

namespace Shortener.Application.Webhooks.Commands.DeleteWebhook;

public sealed class DeleteWebhookHandler(IWebhookRepository webhooks)
{
    public async Task HandleAsync(DeleteWebhookCommand cmd, CancellationToken ct = default)
    {
        var webhook = await webhooks.GetByIdAsync(cmd.WebhookId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Webhook not found.");

        webhook.Deactivate();
        await webhooks.UpdateAsync(webhook, ct);
    }
}
