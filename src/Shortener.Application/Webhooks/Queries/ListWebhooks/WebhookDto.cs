namespace Shortener.Application.Webhooks.Queries.ListWebhooks;

public sealed record WebhookDto(
    Guid Id,
    string Url,
    IReadOnlyList<string> Events,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
