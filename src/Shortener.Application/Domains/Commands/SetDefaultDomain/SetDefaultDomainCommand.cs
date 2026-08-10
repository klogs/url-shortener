namespace Shortener.Application.Domains.Commands.SetDefaultDomain;

public sealed record SetDefaultDomainCommand(Guid DomainId, Guid TenantId);
