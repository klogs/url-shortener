namespace Shortener.Application.Domains.Commands.VerifyDomain;

public sealed record VerifyDomainCommand(Guid DomainId, Guid TenantId);
