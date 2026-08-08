namespace Shortener.Application.Links.Commands.CreateLink;

public sealed record CreateLinkCommand(
    Guid TenantId,
    Guid DomainId,
    string DestinationUrl,
    string? Alias,
    DateTimeOffset? ExpiresAt,
    bool IsAnonymous,
    Guid? CreatedBy,
    string? CaptchaToken,
    string? ClientIp);
