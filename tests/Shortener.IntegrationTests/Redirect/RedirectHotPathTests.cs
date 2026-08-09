using System.Net;
using System.Net.Http.Json;
using Dapper;
using Npgsql;
using Shortener.IntegrationTests.Auth;

namespace Shortener.IntegrationTests.Redirect;

[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class RedirectHotPathTests(IntegrationFixture fixture) : IAsyncLifetime
{
    private ApiIntegrationFactory _apiFactory = null!;
    private RedirectIntegrationFactory _redirectFactory = null!;
    private HttpClient _apiClient = null!;
    private HttpClient _redirectClient = null!;

    public Task InitializeAsync()
    {
        _apiFactory = new ApiIntegrationFactory(fixture);
        _redirectFactory = new RedirectIntegrationFactory(fixture);

        _apiClient = _apiFactory.CreateClient(new() { AllowAutoRedirect = false });
        _apiClient.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, fixture.TestTenantId.ToString());

        _redirectClient = _redirectFactory.CreateClient(new() { AllowAutoRedirect = false });

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _apiClient.Dispose();
        _redirectClient.Dispose();
        await _apiFactory.DisposeAsync();
        await _redirectFactory.DisposeAsync();
    }

    [Fact]
    public async Task Redirect_returns_302_for_active_link()
    {
        var shortCode = await SeedActiveLinkAsync("https://example.com/redirect-test");

        var response = await _redirectClient.GetAsync($"/{shortCode}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://example.com/redirect-test", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Redirect_returns_404_for_unknown_code()
    {
        var response = await _redirectClient.GetAsync("/zzzzzzzz");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Redirect_returns_redirect_on_second_hit_via_cache()
    {
        var shortCode = await SeedActiveLinkAsync("https://example.com/cache-test");

        // First hit — Postgres MISS → sets Redis
        var first = await _redirectClient.GetAsync($"/{shortCode}");
        Assert.Equal(HttpStatusCode.Redirect, first.StatusCode);

        // Second hit — Redis HIT
        var second = await _redirectClient.GetAsync($"/{shortCode}");
        Assert.Equal(HttpStatusCode.Redirect, second.StatusCode);
        Assert.Equal(
            first.Headers.Location?.ToString(),
            second.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Redirect_returns_not_found_for_expired_link()
    {
        var shortCode = await SeedExpiredLinkAsync("https://example.com/expired-test");

        var response = await _redirectClient.GetAsync($"/{shortCode}");

        // Redirect service returns NotFound (404) for expired/disabled links
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<string> SeedActiveLinkAsync(string destinationUrl)
        => await SeedLinkAsync(destinationUrl, expiresAt: null);

    private async Task<string> SeedExpiredLinkAsync(string destinationUrl)
        => await SeedLinkAsync(destinationUrl, expiresAt: DateTimeOffset.UtcNow.AddHours(-1));

    private async Task<string> SeedLinkAsync(string destinationUrl, DateTimeOffset? expiresAt)
    {
        var shortCode = GenerateCode();
        await using var conn = new NpgsqlConnection(fixture.PostgresConnectionString);
        await conn.ExecuteAsync(
            """
            INSERT INTO short_links
                (id, tenant_id, domain_id, short_code, destination_url, status,
                 redirect_type, click_count_snapshot, is_ab_test, has_geo_routes,
                 report_count, created_at_utc, updated_at_utc, expires_at)
            VALUES
                (@Id, @TenantId, @DomainId, @ShortCode, @DestinationUrl, 'Active',
                 302, 0, FALSE, FALSE, 0, NOW(), NOW(), @ExpiresAt)
            """,
            new
            {
                Id = Guid.NewGuid(),
                TenantId = fixture.TestTenantId,
                DomainId = fixture.TestDomainId,
                ShortCode = shortCode,
                DestinationUrl = destinationUrl,
                ExpiresAt = expiresAt,
            });
        return shortCode;
    }

    private static string GenerateCode()
        => Guid.NewGuid().ToString("N")[..7];
}
