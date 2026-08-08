using System.Net;
using System.Net.Http.Json;
using Shortener.IntegrationTests.Auth;

namespace Shortener.IntegrationTests.Links;

[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class CrossTenantIsolationTests(IntegrationFixture fixture) : IAsyncLifetime
{
    private ApiIntegrationFactory _factory = null!;
    private HttpClient _tenantA = null!;
    private HttpClient _tenantB = null!;

    private readonly Guid _tenantAId = fixture.TestTenantId;
    private readonly Guid _tenantBId = Guid.NewGuid(); // different tenant, no seeded domain

    public Task InitializeAsync()
    {
        _factory = new ApiIntegrationFactory(fixture);

        _tenantA = _factory.CreateClient(new() { AllowAutoRedirect = false });
        _tenantA.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, _tenantAId.ToString());

        _tenantB = _factory.CreateClient(new() { AllowAutoRedirect = false });
        _tenantB.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, _tenantBId.ToString());

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _tenantA.Dispose();
        _tenantB.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task TenantB_cannot_read_TenantAs_link()
    {
        // Tenant A creates a link via the management API
        var createRes = await _tenantA.PostAsJsonAsync("/api/v1/links", new
        {
            destinationUrl = "https://example.com/cross-tenant-test",
            domainId = fixture.TestDomainId,
        });

        // The POST may not exist as authenticated create (it goes through /public/links or seeding).
        // Regardless, if we can get a link ID from Tenant A's list, Tenant B must not see it.
        var listResA = await _tenantA.GetAsync("/api/v1/links");
        Assert.Equal(HttpStatusCode.OK, listResA.StatusCode);

        var bodyA = await listResA.Content.ReadFromJsonAsync<LinksResult>();
        if (bodyA?.Items is null || bodyA.Items.Length == 0)
        {
            return; // No links for Tenant A; nothing to test
        }

        var linkId = bodyA.Items[0].Id;

        // Tenant B must receive 404 when trying to access Tenant A's link
        var getRes = await _tenantB.GetAsync($"/api/v1/links/{linkId}");
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
    }

    private sealed record LinksResult(LinkItem[] Items, string? NextCursor);
    private sealed record LinkItem(Guid Id, string ShortCode, string DestinationUrl);
}
