namespace Shortener.Api.Endpoints.Webhooks;

public sealed record CreateWebhookRequest(string Url, IReadOnlyList<string> Events);

public sealed record UpdateWebhookRequest(string Url, IReadOnlyList<string> Events, bool IsActive);
