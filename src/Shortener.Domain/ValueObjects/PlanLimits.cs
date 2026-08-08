namespace Shortener.Domain.ValueObjects;

/// <summary>
/// Per-plan quota caps. -1 means unlimited.
/// </summary>
public sealed record PlanLimits(int MaxLinks, int MaxCustomDomains, int AnalyticsDays);
