using Shortener.Domain.Events;

namespace Shortener.Application.Interfaces;

public interface IClickEventRepository
{
    Task InsertAsync(ClickEvent evt, CancellationToken ct = default);
    Task<IReadOnlyList<DailyClickCount>> GetDailyCountsAsync(
        Guid linkId, Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
}

public sealed record DailyClickCount(DateOnly Date, long Count);
