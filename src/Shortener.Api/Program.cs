using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Shortener.Api.Endpoints.PublicLinks;
using Shortener.Application.Interfaces;
using Shortener.Application.Links.Commands.CreateLink;
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
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection(RateLimitOptions.SectionName));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CreateLinkHandler>();

builder.Services.AddRateLimiter(rateLimiter =>
{
    rateLimiter.AddSlidingWindowLimiter("public-create", options =>
    {
        var rateLimitOptions = builder.Configuration
            .GetSection(RateLimitOptions.SectionName)
            .Get<RateLimitOptions>() ?? new RateLimitOptions();

        options.PermitLimit = rateLimitOptions.PublicCreatePerMinute;
        options.Window = TimeSpan.FromMinutes(1);
        options.SegmentsPerWindow = 6; // 10-second buckets
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    rateLimiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    rateLimiter.OnRejected = (ctx, _) =>
    {
        ctx.HttpContext.Response.Headers.RetryAfter = "60";
        return ValueTask.CompletedTask;
    };
});

var app = builder.Build();

app.UseRateLimiter();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.MapPost("/api/v1/public/links", async (
    CreatePublicLinkRequest request,
    HttpContext ctx,
    CreateLinkHandler handler,
    IOptions<ShortenerOptions> shortenerOpts,
    IDomainRepository domainRepository,
    CancellationToken ct) =>
{
    var normalizedHost = TenantDomain.NormalizeHost(ctx.Request.Host.Value ?? string.Empty);
    var domain = await domainRepository.GetByNormalizedHostAsync(normalizedHost, ct);

    if (domain is null)
    {
        return Results.Problem("Domain not found.", statusCode: StatusCodes.Status404NotFound);
    }

    var clientIp = ctx.Connection.RemoteIpAddress?.ToString();
    var command = new CreateLinkCommand(
        TenantId: domain.TenantId,
        DomainId: domain.Id,
        DestinationUrl: request.DestinationUrl,
        Alias: null,
        ExpiresAt: null,
        IsAnonymous: true,
        CreatedBy: null,
        CaptchaToken: request.CaptchaToken,
        ClientIp: clientIp);

    try
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/v1/links/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
    }
}).RequireRateLimiting("public-create");

app.Run();

// Marker for integration tests
public partial class Program;
