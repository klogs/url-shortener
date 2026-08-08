using System.Net;
using System.Net.Http.Json;
using Shortener.IntegrationTests.Auth;

namespace Shortener.IntegrationTests.Links;

[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class PublicLinkTests(IntegrationFixture fixture) : IAsyncLifetime
{
    private ApiIntegrationFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new ApiIntegrationFactory(fixture);
        _client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task PublicCreate_returns_201_with_shortCode()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/public/links", new
        {
            destinationUrl = "https://example.com/test",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateLinkResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrEmpty(body.ShortCode));
        Assert.Equal("https://example.com/test", body.DestinationUrl);
    }

    [Fact]
    public async Task PublicCreate_with_invalid_url_returns_4xx()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/public/links", new
        {
            destinationUrl = "not-a-url",
        });

        Assert.True((int)response.StatusCode is >= 400 and < 500);
    }

    [Fact]
    public async Task ListLinks_requires_auth_when_not_logged_in()
    {
        var response = await _client.GetAsync("/api/v1/links");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListLinks_returns_200_when_authenticated()
    {
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, fixture.TestTenantId.ToString());

        var response = await _client.GetAsync("/api/v1/links");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record CreateLinkResponse(
        Guid Id, string ShortCode, string ShortUrl, string DestinationUrl, string Status);
}
