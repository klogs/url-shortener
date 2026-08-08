namespace Shortener.Api.Middleware;

internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var headers = ctx.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // HSTS — only meaningful over TLS; skip in local dev
        if (ctx.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        // JSON-only API: tight CSP
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

        await next(ctx);
    }
}
