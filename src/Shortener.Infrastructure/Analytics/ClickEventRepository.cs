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
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

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
            transaction: tx,
            cancellationToken: ct));

        await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE short_links SET click_count_snapshot = click_count_snapshot + 1 WHERE id = @LinkId",
            new { evt.LinkId },
            transaction: tx,
            cancellationToken: ct));

        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<CountryCount>> GetCountryBreakdownAsync(
        Guid linkId, Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync<(string Country, long Count)>(new CommandDefinition(
            """
            SELECT COALESCE(ce.country, 'Unknown') AS country, COUNT(*) AS count
            FROM click_events ce
            JOIN short_links sl ON ce.link_id = sl.id
            WHERE ce.link_id = @LinkId
              AND sl.tenant_id = @TenantId
              AND ce.occurred_at_utc >= @From
              AND ce.occurred_at_utc < @To
            GROUP BY COALESCE(ce.country, 'Unknown')
            ORDER BY count DESC
            LIMIT 20
            """,
            new { LinkId = linkId, TenantId = tenantId, From = from, To = to },
            cancellationToken: ct));
        return rows.Select(r => new CountryCount(r.Country, r.Count)).ToList();
    }

    public async Task<IReadOnlyList<BrowserCount>> GetBrowserBreakdownAsync(
        Guid linkId, Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync<(string? UserAgent, long Count)>(new CommandDefinition(
            """
            SELECT ce.user_agent AS user_agent, COUNT(*) AS count
            FROM click_events ce
            JOIN short_links sl ON ce.link_id = sl.id
            WHERE ce.link_id = @LinkId
              AND sl.tenant_id = @TenantId
              AND ce.occurred_at_utc >= @From
              AND ce.occurred_at_utc < @To
            GROUP BY ce.user_agent
            ORDER BY count DESC
            LIMIT 50
            """,
            new { LinkId = linkId, TenantId = tenantId, From = from, To = to },
            cancellationToken: ct));
        return rows.Select(r => new BrowserCount(r.UserAgent ?? "Unknown", r.Count)).ToList();
    }

    public async Task AnonymizeByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync(new CommandDefinition(
            """
            UPDATE click_events ce
            SET remote_ip = NULL, user_agent = NULL, referer = NULL
            FROM short_links sl
            WHERE ce.link_id = sl.id AND sl.tenant_id = @TenantId
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
            SELECT DATE(ce.occurred_at_utc) AS date, COUNT(*) AS count
            FROM click_events ce
            JOIN short_links sl ON ce.link_id = sl.id
            WHERE ce.link_id = @LinkId
              AND sl.tenant_id = @TenantId
              AND ce.occurred_at_utc >= @From
              AND ce.occurred_at_utc < @To
            GROUP BY DATE(ce.occurred_at_utc)
            ORDER BY date
            """,
            new { LinkId = linkId, TenantId = tenantId, From = from, To = to },
            cancellationToken: ct));

        return rows.Select(r => new DailyClickCount(r.Date, r.Count)).ToList();
    }
}
