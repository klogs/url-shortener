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
}
