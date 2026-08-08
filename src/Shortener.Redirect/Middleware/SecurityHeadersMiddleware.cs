namespace Shortener.Redirect.Middleware;

internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var headers = ctx.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        if (ctx.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        await next(ctx);
    }
}
