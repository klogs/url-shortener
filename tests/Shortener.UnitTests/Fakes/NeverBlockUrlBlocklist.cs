using Shortener.Application.Interfaces;

namespace Shortener.UnitTests.Fakes;

internal sealed class NeverBlockUrlBlocklist : IUrlBlocklist
{
    public bool IsBlocked(string destinationUrl) => false;
}
