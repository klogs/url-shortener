namespace Shortener.Application.Links.Commands.UnblockLink;

public sealed record UnblockLinkCommand(Guid LinkId, Guid TenantId, Guid UnblockedBy);
