using System.Diagnostics.Metrics;

namespace Shortener.Application.Observability;

public sealed class ShortenerMetrics : IDisposable
{
    public const string MeterName = "Shortener";

    private readonly Meter _meter;

    public Counter<long> RedirectOutcomes { get; }
    public Counter<long> CacheOutcomes { get; }

    public ShortenerMetrics()
    {
        _meter = new Meter(MeterName, "1.0.0");
        RedirectOutcomes = _meter.CreateCounter<long>(
            "shortener.redirect.outcomes",
            unit: "{request}",
            description: "Redirect request outcomes by type");
        CacheOutcomes = _meter.CreateCounter<long>(
            "shortener.redirect.cache",
            unit: "{lookup}",
            description: "Redirect cache lookup outcomes");
    }

    public void Dispose() => _meter.Dispose();
}
