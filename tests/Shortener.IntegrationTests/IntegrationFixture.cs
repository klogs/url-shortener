using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shortener.Infrastructure.Persistence;
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

        // Run EF Core migrations
        using var factory = new ApiIntegrationFactory(this);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShortenerDbContext>();
        await db.Database.MigrateAsync();

        // Seed a test tenant and domain
        await SeedTestTenantAsync();
    }

    private async Task SeedTestTenantAsync()
    {
        await using var conn = new NpgsqlConnection(PostgresConnectionString);

        await conn.ExecuteAsync(
            """
            INSERT INTO tenants (id, name, plan, created_at_utc)
            VALUES (@Id, 'Test Tenant', 'Free', NOW())
            ON CONFLICT (id) DO NOTHING
            """,
            new { Id = TestTenantId });

        await conn.ExecuteAsync(
            """
            INSERT INTO tenant_domains (id, tenant_id, host, normalized_host, is_verified, created_at_utc)
            VALUES (@Id, @TenantId, @Host, @Host, TRUE, NOW())
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
