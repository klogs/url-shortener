namespace Shortener.Application.Links.Commands.BlockLink;

public sealed record BlockLinkCommand(Guid LinkId, Guid TenantId, Guid BlockedBy);
