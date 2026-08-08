namespace Shortener.Application.Links.Queries.GetTenantStats;

public sealed record TenantStatsResult(int TotalLinks, int ActiveLinks, long ClicksToday);
