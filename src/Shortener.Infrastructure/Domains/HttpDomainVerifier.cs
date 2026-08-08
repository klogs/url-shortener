using Shortener.Application.Interfaces;

namespace Shortener.Infrastructure.Domains;

internal sealed class HttpDomainVerifier : IDomainVerifier
{
    // Domain verification is rare; a single shared client is sufficient.
    private static readonly HttpClient Client = new(new HttpClientHandler
    {
        AllowAutoRedirect = false,
    })
    {
        Timeout = TimeSpan.FromSeconds(10),
    };

    private const string WellKnownPath = "/.well-known/shortener-verify";

    public async Task<bool> VerifyAsync(string host, string expectedToken, CancellationToken ct = default)
    {
        try
        {
            var url = $"https://{host}{WellKnownPath}";
            using var response = await Client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            return body.Contains(expectedToken, StringComparison.Ordinal);
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }
}
