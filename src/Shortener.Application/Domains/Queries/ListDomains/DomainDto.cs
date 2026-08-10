namespace Shortener.Application.Domains.Queries.ListDomains;

public sealed record DomainDto(
    Guid Id,
    string Host,
    string NormalizedHost,
    string Status,
    bool IsDefault,
    bool IsVerified,
    string? VerificationToken,
    DateTimeOffset? VerifiedAt,
    DateTimeOffset CreatedAtUtc);
