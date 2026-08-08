using Shortener.Domain.Enums;

namespace Shortener.Application.Links.Commands.UpdateLink;

public sealed record UpdateLinkCommand(
    Guid Id,
    Guid TenantId,
    Guid UpdatedBy,
    string DestinationUrl,
    string? Title,
    DateTimeOffset? ExpiresAt,
    RedirectType RedirectType);
