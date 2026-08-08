namespace Shortener.Application.Links.Commands.DeleteLink;

public sealed record DeleteLinkCommand(Guid Id, Guid TenantId, Guid DeletedBy);
