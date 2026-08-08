namespace Shortener.Domain.Events;

public sealed record ClickEvent(
    Guid LinkId,
    Guid TenantId,
    string ShortCode,
    string NormalizedHost,
    DateTimeOffset OccurredAtUtc,
    string? UserAgent,
    string? RemoteIp,
    string? Referer,
    string? Country);
