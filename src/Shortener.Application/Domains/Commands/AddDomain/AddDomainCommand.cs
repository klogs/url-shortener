namespace Shortener.Application.Domains.Commands.AddDomain;

public sealed record AddDomainCommand(Guid TenantId, string Host);
