namespace Shortener.Application.Interfaces;

public interface IUrlBlocklist
{
    /// <summary>Returns true if the destination URL's host is on the blocked list.</summary>
    bool IsBlocked(string destinationUrl);
}
