namespace Shortener.Application.Webhooks.Commands.CreateWebhook;

public sealed record CreateWebhookCommand(
    Guid TenantId,
    string Url,
    IReadOnlyList<string> Events);
