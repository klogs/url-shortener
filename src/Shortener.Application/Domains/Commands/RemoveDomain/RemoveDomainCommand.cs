namespace Shortener.Application.Domains.Commands.RemoveDomain;

public sealed record RemoveDomainCommand(Guid DomainId, Guid TenantId);
