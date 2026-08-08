namespace Shortener.Application.Webhooks.Commands.UpdateWebhook;

public sealed record UpdateWebhookCommand(
    Guid WebhookId,
    Guid TenantId,
    string Url,
    IReadOnlyList<string> Events,
    bool IsActive);
