using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class AbuseReportRepository(ShortenerDbContext db) : IAbuseReportRepository
{
    public async Task InsertAsync(AbuseReport report, CancellationToken ct)
    {
        db.AbuseReports.Add(report);
        await db.SaveChangesAsync(ct);
    }

    public Task<int> CountByLinkAsync(Guid linkId, CancellationToken ct)
        => db.AbuseReports.CountAsync(r => r.LinkId == linkId, ct);
}
