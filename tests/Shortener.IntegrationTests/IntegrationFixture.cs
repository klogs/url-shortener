using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shortener.Infrastructure.Persistence;
using Shortener.Migrator.Migrations;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Shortener.IntegrationTests;

public sealed class IntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public string PostgresConnectionString { get; private set; } = string.Empty;
    public string RedisConnectionString { get; private set; } = string.Empty;

    // Shared test tenant + domain seeded once for the collection
    public Guid TestTenantId { get; } = Guid.NewGuid();
    public Guid TestDomainId { get; } = Guid.NewGuid();
    public const string TestHost = "localhost";

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        PostgresConnectionString = _postgres.GetConnectionString();
        RedisConnectionString = _redis.GetConnectionString();

        // Run migrations BEFORE creating any WebApplicationFactory so that
        // SingleTenantSeeder (a hosted service) does not query tables that
        // don't exist yet.
        var opts = new DbContextOptionsBuilder<ShortenerDbContext>()
            .UseNpgsql(PostgresConnectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(InitialSchema).Assembly.FullName))
            .Options;
        await using var db = new ShortenerDbContext(opts);
        await db.Database.MigrateAsync();

        // Seed a test tenant and domain
        await SeedTestTenantAsync();
    }

    private async Task SeedTestTenantAsync()
    {
        await using var conn = new NpgsqlConnection(PostgresConnectionString);

        await conn.ExecuteAsync(
            """
            INSERT INTO tenants (id, name, plan, is_active, created_at_utc)
            VALUES (@Id, 'Test Tenant', 0, TRUE, NOW())
            ON CONFLICT (id) DO NOTHING
            """,
            new { Id = TestTenantId });

        await conn.ExecuteAsync(
            """
            INSERT INTO domains (id, tenant_id, host, normalized_host, status, is_default, created_at_utc)
            VALUES (@Id, @TenantId, @Host, @Host, 'Active', TRUE, NOW())
            ON CONFLICT (id) DO NOTHING
            """,
            new { Id = TestDomainId, TenantId = TestTenantId, Host = TestHost });
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}
