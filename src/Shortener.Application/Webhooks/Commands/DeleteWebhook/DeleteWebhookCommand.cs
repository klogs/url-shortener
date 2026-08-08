namespace Shortener.Application.Webhooks.Commands.DeleteWebhook;

public sealed record DeleteWebhookCommand(Guid WebhookId, Guid TenantId);
