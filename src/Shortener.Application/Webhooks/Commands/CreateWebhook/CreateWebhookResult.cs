namespace Shortener.Application.Webhooks.Commands.CreateWebhook;

public sealed record CreateWebhookResult(
    Guid Id,
    string Url,
    IReadOnlyList<string> Events,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
