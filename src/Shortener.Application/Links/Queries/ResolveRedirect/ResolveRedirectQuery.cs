namespace Shortener.Application.Links.Queries.ResolveRedirect;

public sealed record ResolveRedirectQuery(string NormalizedHost, string ShortCode, string? ClientIp = null);
