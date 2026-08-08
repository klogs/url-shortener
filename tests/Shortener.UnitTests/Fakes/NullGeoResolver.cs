using Shortener.Application.Interfaces;

namespace Shortener.UnitTests.Fakes;

internal sealed class NullGeoResolver : IGeoResolver
{
    public Task<string?> GetCountryAsync(string ip, CancellationToken ct = default)
        => Task.FromResult<string?>(null);
}
