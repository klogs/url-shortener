using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Domain.Enums;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class LinkStatsRepository(
    IDbContextFactory<ShortenerDbContext> dbFactory,
    IOptions<DatabaseOptions> dbOpts) : ILinkStatsRepository
{
    private readonly string _connectionString = dbOpts.Value.ConnectionString;

    public async Task<int> CountTotalAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.ShortLinks
            .CountAsync(l => l.TenantId == tenantId && l.Status != LinkStatus.Deleted, ct);
    }

    public async Task<int> CountActiveAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.ShortLinks
            .CountAsync(l => l.TenantId == tenantId && l.Status == LinkStatus.Active, ct);
    }

    public async Task<long> CountClicksTodayAsync(Guid tenantId, CancellationToken ct = default)
    {
        var today = DateTimeOffset.UtcNow.Date;
        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.ExecuteScalarAsync<long>(new CommandDefinition(
            """
            SELECT COUNT(*)
            FROM click_events ce
            JOIN short_links sl ON ce.link_id = sl.id
            WHERE sl.tenant_id = @TenantId AND ce.occurred_at_utc >= @Today
            """,
            new { TenantId = tenantId, Today = today },
            cancellationToken: ct));
    }
}
