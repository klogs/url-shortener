using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Domain.Events;

namespace Shortener.Infrastructure.Analytics;

internal sealed class ClickEventRepository(IOptions<DatabaseOptions> dbOpts) : IClickEventRepository
{
    private readonly string _connectionString = dbOpts.Value.ConnectionString;

    public async Task InsertAsync(ClickEvent evt, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO click_events
                (link_id, tenant_id, short_code, normalized_host,
                 occurred_at_utc, user_agent, remote_ip, referer, country)
            VALUES
                (@LinkId, @TenantId, @ShortCode, @NormalizedHost,
                 @OccurredAtUtc, @UserAgent, @RemoteIp, @Referer, @Country)
            """,
            new
            {
                evt.LinkId,
                evt.TenantId,
                evt.ShortCode,
                evt.NormalizedHost,
                evt.OccurredAtUtc,
                evt.UserAgent,
                evt.RemoteIp,
                evt.Referer,
                evt.Country,
            },
            cancellationToken: ct));
    }

    public async Task AnonymizeByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync(new CommandDefinition(
            """
            UPDATE click_events
            SET remote_ip = NULL, user_agent = NULL, referer = NULL
            WHERE tenant_id = @TenantId
            """,
            new { TenantId = tenantId },
            cancellationToken: ct));
    }

    public async Task<IReadOnlyList<DailyClickCount>> GetDailyCountsAsync(
        Guid linkId, Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync<(DateOnly Date, long Count)>(new CommandDefinition(
            """
            SELECT DATE(occurred_at_utc) AS date, COUNT(*) AS count
            FROM click_events
            WHERE link_id = @LinkId
              AND tenant_id = @TenantId
              AND occurred_at_utc >= @From
              AND occurred_at_utc < @To
            GROUP BY DATE(occurred_at_utc)
            ORDER BY date
            """,
            new { LinkId = linkId, TenantId = tenantId, From = from, To = to },
            cancellationToken: ct));

        return rows.Select(r => new DailyClickCount(r.Date, r.Count)).ToList();
    }
}
