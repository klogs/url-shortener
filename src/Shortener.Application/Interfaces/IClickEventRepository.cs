using Shortener.Domain.Events;

namespace Shortener.Application.Interfaces;

public interface IClickEventRepository
{
    Task InsertAsync(ClickEvent evt, CancellationToken ct = default);
    Task<IReadOnlyList<DailyClickCount>> GetDailyCountsAsync(
        Guid linkId, Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<IReadOnlyList<CountryCount>> GetCountryBreakdownAsync(
        Guid linkId, Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<IReadOnlyList<BrowserCount>> GetBrowserBreakdownAsync(
        Guid linkId, Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task AnonymizeByTenantAsync(Guid tenantId, CancellationToken ct = default);
}

public sealed record DailyClickCount(DateOnly Date, long Count);
public sealed record CountryCount(string Country, long Count);
public sealed record BrowserCount(string Browser, long Count);
