using Microsoft.AspNetCore.Http;

namespace Shortener.Infrastructure.Http;

/// <summary>
/// Forwards the Bearer token from the current HTTP request to outgoing HTTP client calls.
/// No-op when called outside an HTTP context (e.g. background Worker services).
/// </summary>
internal sealed class ForwardAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true
            && httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", (string?)authHeader);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
