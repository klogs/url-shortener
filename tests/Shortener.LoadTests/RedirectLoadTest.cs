using System.Diagnostics;
using System.Net;

namespace Shortener.LoadTests;

/// <summary>
/// BCL HttpClient fallback load test when k6 is unavailable.
/// Run with: dotnet test --filter "Category=Load"
/// Set env vars: LOAD_BASE_URL, LOAD_SHORT_CODE, LOAD_CONCURRENCY, LOAD_REQUESTS
/// </summary>
[Trait("Category", "Load")]
public sealed class RedirectLoadTest
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("LOAD_BASE_URL") ?? "http://localhost:5001";

    private static readonly string ShortCode =
        Environment.GetEnvironmentVariable("LOAD_SHORT_CODE") ?? "testcode";

    private static readonly int Concurrency =
        int.TryParse(Environment.GetEnvironmentVariable("LOAD_CONCURRENCY"), out var c) ? c : 50;

    private static readonly int TotalRequests =
        int.TryParse(Environment.GetEnvironmentVariable("LOAD_REQUESTS"), out var r) ? r : 500;

    [Fact(Skip = "Load test — run explicitly with LOAD_BASE_URL set")]
    public async Task Redirect_hot_path_meets_latency_thresholds()
    {
        using var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            MaxConnectionsPerServer = Concurrency,
        };
        using var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };

        var latencies = new long[TotalRequests];
        var errors = 0;

        var semaphore = new SemaphoreSlim(Concurrency);
        var tasks = Enumerable.Range(0, TotalRequests).Select(async i =>
        {
            await semaphore.WaitAsync();
            try
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync($"/{ShortCode}");
                sw.Stop();

                latencies[i] = sw.ElapsedMilliseconds;

                if (response.StatusCode is not (HttpStatusCode.Redirect
                    or HttpStatusCode.MovedPermanently
                    or HttpStatusCode.TemporaryRedirect
                    or HttpStatusCode.PermanentRedirect))
                {
                    Interlocked.Increment(ref errors);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);

        Array.Sort(latencies);
        var p95 = latencies[(int)(TotalRequests * 0.95)];
        var p99 = latencies[(int)(TotalRequests * 0.99)];
        var errorRate = (double)errors / TotalRequests;

        Assert.True(p95 < 200, $"p95={p95}ms exceeds 200ms threshold");
        Assert.True(errorRate < 0.01, $"error_rate={errorRate:P2} exceeds 1% threshold");

        // Output summary (visible with dotnet test -v normal)
        Console.WriteLine($"Requests={TotalRequests}  Concurrency={Concurrency}");
        Console.WriteLine($"p95={p95}ms  p99={p99}ms  errors={errors} ({errorRate:P2})");
    }
}
