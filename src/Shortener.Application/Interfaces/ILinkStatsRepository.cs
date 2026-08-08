namespace Shortener.Application.Interfaces;

public interface ILinkStatsRepository
{
    Task<int> CountTotalAsync(Guid tenantId, CancellationToken ct = default);
    Task<int> CountActiveAsync(Guid tenantId, CancellationToken ct = default);
    Task<long> CountClicksTodayAsync(Guid tenantId, CancellationToken ct = default);
}
