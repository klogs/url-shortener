namespace Shortener.Application.Variants.Queries;

public sealed record ListVariantsQuery(Guid LinkId, Guid TenantId);
