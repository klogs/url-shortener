using System.Net;
using MaxMind.Db;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;

namespace Shortener.Infrastructure.Geo;

public sealed class MaxMindGeoResolver : IGeoResolver, IDisposable
{
    private readonly Reader? _reader;
    private readonly ILogger<MaxMindGeoResolver> _logger;

    public MaxMindGeoResolver(IOptions<GeoOptions> options, ILogger<MaxMindGeoResolver> logger)
    {
        _logger = logger;
        var path = options.Value.DatabasePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _logger.LogWarning("GeoIP database not found at '{Path}'. Geo routing will be skipped.", path);
            return;
        }

        try
        {
            _reader = new Reader(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open GeoIP database at '{Path}'. Geo routing will be skipped.", path);
        }
    }

    public Task<string?> GetCountryAsync(string ip, CancellationToken ct = default)
    {
        if (_reader is null || string.IsNullOrWhiteSpace(ip))
        {
            return Task.FromResult<string?>(null);
        }

        if (!IPAddress.TryParse(ip, out var address))
        {
            return Task.FromResult<string?>(null);
        }

        try
        {
            var record = _reader.Find<Dictionary<string, object>>(address);
            if (record is null)
            {
                return Task.FromResult<string?>(null);
            }

            if (record.TryGetValue("country", out var countryObj)
                && countryObj is Dictionary<string, object> country
                && country.TryGetValue("iso_code", out var isoCode)
                && isoCode is string code)
            {
                return Task.FromResult<string?>(code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "GeoIP lookup failed for IP {Ip}.", ip);
        }

        return Task.FromResult<string?>(null);
    }

    public void Dispose() => _reader?.Dispose();
}
