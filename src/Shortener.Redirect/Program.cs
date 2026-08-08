using Serilog;
using Serilog.Events;
using Shortener.Application.Interfaces;
using Shortener.Application.Links.Queries.ResolveRedirect;
using Shortener.Application.Options;
using Shortener.Domain.Entities;
using Shortener.Domain.Events;
using Shortener.Infrastructure;
using Shortener.Redirect.Middleware;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "shortener-redirect")
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "shortener-redirect")
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter()));

builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<ShortenerOptions>(
    builder.Configuration.GetSection(ShortenerOptions.SectionName));
builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAnalyticsPipeline(builder.Configuration);
builder.Services.AddScoped<ResolveRedirectHandler>();

var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "REDIRECT {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    opts.GetLevel = (ctx, elapsed, ex) =>
        ex is null && ctx.Response.StatusCode < 500
            ? LogEventLevel.Information
            : LogEventLevel.Error;
});

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.MapGet("/{shortCode}", async (
    string shortCode,
    HttpContext ctx,
    ResolveRedirectHandler handler,
    IClickEventBuffer analyticsBuffer,
    IRedirectRateLimiter rateLimiter,
    TimeProvider time,
    CancellationToken ct) =>
{
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    if (!await rateLimiter.IsAllowedAsync(ip, ct))
    {
        ctx.Response.Headers.RetryAfter = "60";
        return Results.StatusCode(StatusCodes.Status429TooManyRequests);
    }

    var normalizedHost = TenantDomain.NormalizeHost(ctx.Request.Host.Value ?? string.Empty);
    var query = new ResolveRedirectQuery(normalizedHost, shortCode, ip);
    var resolution = await handler.HandleAsync(query, ct);

    if (resolution.Outcome != RedirectOutcome.Redirect)
    {
        return Results.NotFound();
    }

    // Non-blocking analytics — TryWrite returns false if buffer is full (dropped, never throws)
    analyticsBuffer.TryWrite(new ClickEvent(
        LinkId: resolution.LinkId ?? Guid.Empty,
        TenantId: resolution.TenantId ?? Guid.Empty,
        ShortCode: shortCode,
        NormalizedHost: normalizedHost,
        OccurredAtUtc: time.GetUtcNow(),
        UserAgent: ctx.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null,
        RemoteIp: ctx.Connection.RemoteIpAddress?.ToString(),
        Referer: ctx.Request.Headers.Referer.ToString() is { Length: > 0 } ref_ ? ref_ : null,
        Country: null));

    var url = resolution.DestinationUrl!;
    return resolution.StatusCode switch
    {
        301 => Results.Redirect(url, permanent: true,  preserveMethod: false),
        307 => Results.Redirect(url, permanent: false, preserveMethod: true),
        308 => Results.Redirect(url, permanent: true,  preserveMethod: true),
        _   => Results.Redirect(url, permanent: false, preserveMethod: false), // 302
    };
});

app.Run();

// Marker for integration tests
public partial class Program;
