namespace Shortener.Api.Endpoints.Abuse;

public sealed record CreateAbuseReportRequest(string ShortCode, string? Reason);
