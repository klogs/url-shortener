namespace Shortener.Application.Links.Queries.GetLinkAnalytics;

public sealed record GetLinkAnalyticsQuery(Guid LinkId, Guid TenantId, int Days = 30);
