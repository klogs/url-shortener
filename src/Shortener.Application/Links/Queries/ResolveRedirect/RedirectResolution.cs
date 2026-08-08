namespace Shortener.Application.Links.Queries.ResolveRedirect;

public enum RedirectOutcome { Redirect, NotFound, Expired, Disabled }

public sealed record RedirectResolution(
    RedirectOutcome Outcome,
    string? DestinationUrl = null,
    int StatusCode = 302);
