using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Domain.Enums;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class LinkStatsRepository(
    ShortenerDbContext db,
    IOptions<DatabaseOptions> dbOpts) : ILinkStatsRepository
{
    private readonly string _connectionString = dbOpts.Value.ConnectionString;

    public Task<int> CountTotalAsync(Guid tenantId, CancellationToken ct = default)
        => db.ShortLinks
            .CountAsync(l => l.TenantId == tenantId && l.Status != LinkStatus.Deleted, ct);

    public Task<int> CountActiveAsync(Guid tenantId, CancellationToken ct = default)
        => db.ShortLinks
            .CountAsync(l => l.TenantId == tenantId && l.Status == LinkStatus.Active, ct);

    public async Task<long> CountClicksTodayAsync(Guid tenantId, CancellationToken ct = default)
    {
        var today = DateTimeOffset.UtcNow.Date;
        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) FROM click_events WHERE tenant_id = @TenantId AND occurred_at_utc >= @Today",
            new { TenantId = tenantId, Today = today },
            cancellationToken: ct));
    }
}
