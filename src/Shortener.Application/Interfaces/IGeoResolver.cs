namespace Shortener.Application.Interfaces;

public interface IGeoResolver
{
    /// <summary>
    /// Returns ISO 3166-1 alpha-2 country code for the given IP, or null if unknown / resolver unavailable.
    /// </summary>
    Task<string?> GetCountryAsync(string ip, CancellationToken ct = default);
}
