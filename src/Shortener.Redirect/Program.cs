using Shortener.Application.Links.Queries.ResolveRedirect;
using Shortener.Application.Options;
using Shortener.Domain.Entities;
using Shortener.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<ShortenerOptions>(
    builder.Configuration.GetSection(ShortenerOptions.SectionName));
builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ResolveRedirectHandler>();

var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.MapGet("/{shortCode}", async (
    string shortCode,
    HttpContext ctx,
    ResolveRedirectHandler handler,
    CancellationToken ct) =>
{
    var normalizedHost = TenantDomain.NormalizeHost(ctx.Request.Host.Value ?? string.Empty);
    var query = new ResolveRedirectQuery(normalizedHost, shortCode);
    var resolution = await handler.HandleAsync(query, ct);

    if (resolution.Outcome != RedirectOutcome.Redirect)
    {
        return Results.NotFound();
    }

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
