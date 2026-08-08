using Shortener.Domain.Enums;
using Shortener.Domain.ValueObjects;

namespace Shortener.Application.Billing;

public static class PlanLimitsProvider
{
    private static readonly Dictionary<TenantPlan, PlanLimits> Limits = new()
    {
        [TenantPlan.Free]       = new PlanLimits(MaxLinks: 10,   MaxCustomDomains: 0,  AnalyticsDays: 30),
        [TenantPlan.Pro]        = new PlanLimits(MaxLinks: 500,  MaxCustomDomains: 3,  AnalyticsDays: 90),
        [TenantPlan.Enterprise] = new PlanLimits(MaxLinks: -1,   MaxCustomDomains: -1, AnalyticsDays: 365),
    };

    public static PlanLimits For(TenantPlan plan) => Limits[plan];
}
