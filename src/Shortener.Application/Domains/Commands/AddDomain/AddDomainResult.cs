namespace Shortener.Application.Domains.Commands.AddDomain;

public sealed record AddDomainResult(
    Guid Id,
    string Host,
    string NormalizedHost,
    string Status,
    bool IsDefault,
    string VerificationToken);
