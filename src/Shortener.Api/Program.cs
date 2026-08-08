using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Shortener.Api.Endpoints.Domains;
using Shortener.Api.Endpoints.Links;
using Shortener.Api.Endpoints.PublicLinks;
using Shortener.Application.Domains.Commands.AddDomain;
using Shortener.Application.Domains.Commands.RemoveDomain;
using Shortener.Application.Domains.Commands.VerifyDomain;
using Shortener.Application.Domains.Queries.ListDomains;
using Shortener.Application.Interfaces;
using Shortener.Application.Links.Commands.CreateLink;
using Shortener.Application.Links.Commands.DeleteLink;
using Shortener.Application.Links.Commands.UpdateLink;
using Shortener.Application.Links.Queries.GetLink;
using Shortener.Application.Links.Queries.ListLinks;
using Shortener.Application.Options;
using Shortener.Domain.Entities;
using Shortener.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);

// Options
builder.Services.Configure<ShortenerOptions>(
    builder.Configuration.GetSection(ShortenerOptions.SectionName));
builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection(RateLimitOptions.SectionName));
builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection(AuthOptions.SectionName));

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Handlers
builder.Services.AddScoped<CreateLinkHandler>();
builder.Services.AddScoped<ListLinksHandler>();
builder.Services.AddScoped<GetLinkHandler>();
builder.Services.AddScoped<UpdateLinkHandler>();
builder.Services.AddScoped<DeleteLinkHandler>();

// Domain handlers
builder.Services.AddScoped<AddDomainHandler>();
builder.Services.AddScoped<ListDomainsHandler>();
builder.Services.AddScoped<VerifyDomainHandler>();
builder.Services.AddScoped<RemoveDomainHandler>();

// JWT bearer auth
var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>();
if (authOptions is not null && !string.IsNullOrWhiteSpace(authOptions.Authority))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.Authority = authOptions.Authority;
            opts.Audience = authOptions.ClientId;
            opts.MapInboundClaims = false;
        });
    builder.Services.AddAuthorization();
}
else
{
    // Auth not configured — allow anonymous for local dev
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}

// Rate limiting
builder.Services.AddRateLimiter(rateLimiter =>
{
    rateLimiter.AddSlidingWindowLimiter("public-create", options =>
    {
        var rateLimitOptions = builder.Configuration
            .GetSection(RateLimitOptions.SectionName)
            .Get<RateLimitOptions>() ?? new RateLimitOptions();

        options.PermitLimit = rateLimitOptions.PublicCreatePerMinute;
        options.Window = TimeSpan.FromMinutes(1);
        options.SegmentsPerWindow = 6;
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

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// ── Public: anonymous link creation ──────────────────────────────────────────

app.MapPost("/api/v1/public/links", async (
    CreatePublicLinkRequest request,
    HttpContext ctx,
    CreateLinkHandler handler,
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

// ── Authenticated: link CRUD ──────────────────────────────────────────────────

var links = app.MapGroup("/api/v1/links").RequireAuthorization();

links.MapGet("/", async (
    ClaimsPrincipal user,
    ListLinksHandler handler,
    int pageSize = 20,
    Guid? after = null,
    CancellationToken ct = default) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    var result = await handler.HandleAsync(new ListLinksQuery(tenantId.Value, pageSize, after), ct);
    return Results.Ok(result);
});

links.MapGet("/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    GetLinkHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    var result = await handler.HandleAsync(new GetLinkQuery(id, tenantId.Value), ct);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

links.MapPut("/{id:guid}", async (
    Guid id,
    UpdateLinkRequest request,
    ClaimsPrincipal user,
    UpdateLinkHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    var userId = ResolveUserId(user);
    if (tenantId is null || userId is null)
    {
        return Results.Forbid();
    }

    var command = new UpdateLinkCommand(
        id, tenantId.Value, userId.Value,
        request.DestinationUrl, request.Title, request.ExpiresAt, request.RedirectType);

    try
    {
        await handler.HandleAsync(command, ct);
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
    }
});

links.MapDelete("/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    DeleteLinkHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    var userId = ResolveUserId(user);
    if (tenantId is null || userId is null)
    {
        return Results.Forbid();
    }

    try
    {
        await handler.HandleAsync(new DeleteLinkCommand(id, tenantId.Value, userId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

// ── Authenticated: domain management ─────────────────────────────────────────

var domains = app.MapGroup("/api/v1/domains").RequireAuthorization();

domains.MapGet("/", async (
    ClaimsPrincipal user,
    ListDomainsHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    var result = await handler.HandleAsync(new ListDomainsQuery(tenantId.Value), ct);
    return Results.Ok(result);
});

domains.MapPost("/", async (
    AddDomainRequest request,
    ClaimsPrincipal user,
    AddDomainHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        var result = await handler.HandleAsync(new AddDomainCommand(tenantId.Value, request.Host), ct);
        return Results.Created($"/api/v1/domains/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
    }
});

domains.MapPost("/{id:guid}/verify", async (
    Guid id,
    ClaimsPrincipal user,
    VerifyDomainHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        await handler.HandleAsync(new VerifyDomainCommand(id, tenantId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException ex)
    {
        var status = ex.Message.Contains("not found") ? StatusCodes.Status404NotFound : StatusCodes.Status422UnprocessableEntity;
        return Results.Problem(ex.Message, statusCode: status);
    }
});

domains.MapDelete("/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    RemoveDomainHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        await handler.HandleAsync(new RemoveDomainCommand(id, tenantId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException ex)
    {
        var status = ex.Message.Contains("not found") ? StatusCodes.Status404NotFound : StatusCodes.Status422UnprocessableEntity;
        return Results.Problem(ex.Message, statusCode: status);
    }
});

app.Run();

static Guid? ResolveTenantId(ClaimsPrincipal user)
{
    // TenantId is stored in "tid" claim (Klogs IdP convention) or falls back to "sub"
    var tid = user.FindFirstValue("tid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    return Guid.TryParse(tid, out var id) ? id : null;
}

static Guid? ResolveUserId(ClaimsPrincipal user)
{
    var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    return Guid.TryParse(sub, out var id) ? id : null;
}

// Marker for integration tests
public partial class Program;
