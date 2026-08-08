using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface IAbuseReportRepository
{
    Task InsertAsync(AbuseReport report, CancellationToken ct = default);
    Task<int> CountByLinkAsync(Guid linkId, CancellationToken ct = default);
}
