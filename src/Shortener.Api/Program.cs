using System.Security.Claims;
using System.Threading.RateLimiting;
using QRCoder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Shortener.Api.Auth;
using Shortener.Api.Endpoints.Abuse;
using Shortener.Api.Endpoints.ApiKeys;
using Shortener.Api.Endpoints.Domains;
using Shortener.Api.Endpoints.Links;
using Shortener.Api.Endpoints.PublicLinks;
using Shortener.Api.Endpoints.Webhooks;
using Shortener.Application.Abuse.Commands.CreateAbuseReport;
using Shortener.Application.ApiKeys.Commands.CreateApiKey;
using Shortener.Application.ApiKeys.Commands.RevokeApiKey;
using Shortener.Application.ApiKeys.Queries.ListApiKeys;
using Shortener.Application.Domains.Commands.AddDomain;
using Shortener.Application.Domains.Commands.RemoveDomain;
using Shortener.Application.Domains.Commands.VerifyDomain;
using Shortener.Application.Domains.Queries.ListDomains;
using Shortener.Application.Interfaces;
using Shortener.Application.Links.Commands.BulkCreateLinks;
using Shortener.Application.Links.Commands.CreateLink;
using Shortener.Application.Links.Commands.BlockLink;
using Shortener.Application.Links.Commands.DeleteLink;
using Shortener.Application.Links.Commands.UnblockLink;
using Shortener.Application.Links.Commands.UpdateLink;
using Shortener.Application.Links.Queries.GetLink;
using Shortener.Application.Links.Queries.ListLinks;
using Shortener.Application.Options;
using Shortener.Application.GeoRoutes.Commands.CreateGeoRoute;
using Shortener.Application.GeoRoutes.Commands.DeleteGeoRoute;
using Shortener.Application.GeoRoutes.Queries;
using Shortener.Application.Variants.Commands.CreateVariant;
using Shortener.Application.Variants.Commands.DeleteVariant;
using Shortener.Application.Variants.Queries;
using Shortener.Application.Webhooks.Commands.CreateWebhook;
using Shortener.Application.Webhooks.Commands.DeleteWebhook;
using Shortener.Application.Webhooks.Commands.UpdateWebhook;
using Shortener.Application.Webhooks.Queries.ListWebhooks;
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

// API key handlers
builder.Services.AddScoped<CreateApiKeyHandler>();
builder.Services.AddScoped<ListApiKeysHandler>();
builder.Services.AddScoped<RevokeApiKeyHandler>();

// Variant (A/B) handlers
builder.Services.AddScoped<CreateVariantHandler>();
builder.Services.AddScoped<DeleteVariantHandler>();
builder.Services.AddScoped<ListVariantsHandler>();

// Geo route handlers
builder.Services.AddScoped<CreateGeoRouteHandler>();
builder.Services.AddScoped<DeleteGeoRouteHandler>();
builder.Services.AddScoped<ListGeoRoutesHandler>();

// Block / unblock / abuse report handlers
builder.Services.AddScoped<BlockLinkHandler>();
builder.Services.AddScoped<UnblockLinkHandler>();
builder.Services.AddScoped<CreateAbuseReportHandler>();

// Bulk create handler
builder.Services.AddScoped<BulkCreateLinksHandler>();

// Webhook handlers
builder.Services.AddScoped<CreateWebhookHandler>();
builder.Services.AddScoped<ListWebhooksHandler>();
builder.Services.AddScoped<UpdateWebhookHandler>();
builder.Services.AddScoped<DeleteWebhookHandler>();

// Authentication: JWT bearer (primary) + API key (secondary)
var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>();
if (authOptions is not null && !string.IsNullOrWhiteSpace(authOptions.Authority))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.Authority = authOptions.Authority;
            opts.Audience = authOptions.ClientId;
            opts.MapInboundClaims = false;
        })
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationOptions.SchemeName, _ => { });
}
else
{
    // Auth not configured — allow anonymous for local dev
    builder.Services.AddAuthentication()
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationOptions.SchemeName, _ => { });
}

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("ApiKeyLinksWrite", policy =>
        policy.AddAuthenticationSchemes(ApiKeyAuthenticationOptions.SchemeName)
              .RequireAuthenticatedUser()
              .RequireClaim("scope", "links:write"));
});

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

    rateLimiter.AddSlidingWindowLimiter("public-reports", options =>
    {
        var rateLimitOptions = builder.Configuration
            .GetSection(RateLimitOptions.SectionName)
            .Get<RateLimitOptions>() ?? new RateLimitOptions();

        options.PermitLimit = rateLimitOptions.PublicReportPerMinute;
        options.Window = TimeSpan.FromMinutes(1);
        options.SegmentsPerWindow = 4;
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

// ── Authenticated: API key management ────────────────────────────────────────

var apiKeys = app.MapGroup("/api/v1/api-keys").RequireAuthorization();

apiKeys.MapGet("/", async (
    ClaimsPrincipal user,
    ListApiKeysHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    var result = await handler.HandleAsync(new ListApiKeysQuery(tenantId.Value), ct);
    return Results.Ok(result);
});

apiKeys.MapPost("/", async (
    CreateApiKeyRequest request,
    ClaimsPrincipal user,
    CreateApiKeyHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        var command = new CreateApiKeyCommand(tenantId.Value, request.Name, request.Scopes, request.ExpiresAt);
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/v1/api-keys/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
});

apiKeys.MapDelete("/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    RevokeApiKeyHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        await handler.HandleAsync(new RevokeApiKeyCommand(id, tenantId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

// ── Bulk create (API key with links:write scope) ──────────────────────────────

app.MapPost("/api/v1/links/bulk", async (
    BulkCreateRequest request,
    ClaimsPrincipal user,
    BulkCreateLinksHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    var userId = ResolveUserId(user);
    if (tenantId is null || userId is null)
    {
        return Results.Forbid();
    }

    var items = request.Links
        .Select(l => new BulkLinkItem(l.DestinationUrl, l.Alias, l.ExpiresAt))
        .ToList();

    try
    {
        var command = new BulkCreateLinksCommand(tenantId.Value, request.DomainId, userId.Value, items);
        var result = await handler.HandleAsync(command, ct);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
    }
}).RequireAuthorization("ApiKeyLinksWrite");

// ── Authenticated: webhook management ────────────────────────────────────────

var webhooks = app.MapGroup("/api/v1/webhooks").RequireAuthorization();

webhooks.MapGet("/", async (
    ClaimsPrincipal user,
    ListWebhooksHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    var result = await handler.HandleAsync(new ListWebhooksQuery(tenantId.Value), ct);
    return Results.Ok(result);
});

webhooks.MapPost("/", async (
    CreateWebhookRequest request,
    ClaimsPrincipal user,
    CreateWebhookHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        var command = new CreateWebhookCommand(tenantId.Value, request.Url, request.Events);
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/v1/webhooks/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
});

webhooks.MapPut("/{id:guid}", async (
    Guid id,
    UpdateWebhookRequest request,
    ClaimsPrincipal user,
    UpdateWebhookHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        var command = new UpdateWebhookCommand(id, tenantId.Value, request.Url, request.Events, request.IsActive);
        await handler.HandleAsync(command, ct);
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

webhooks.MapDelete("/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    DeleteWebhookHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        await handler.HandleAsync(new DeleteWebhookCommand(id, tenantId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

// ── Authenticated: A/B variants ──────────────────────────────────────────────

links.MapGet("/{id:guid}/variants", async (
    Guid id,
    ClaimsPrincipal user,
    ListVariantsHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    var result = await handler.HandleAsync(new ListVariantsQuery(id, tenantId.Value), ct);
    return Results.Ok(result);
});

links.MapPost("/{id:guid}/variants", async (
    Guid id,
    CreateVariantRequest request,
    ClaimsPrincipal user,
    CreateVariantHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        var command = new CreateVariantCommand(id, tenantId.Value, request.Label, request.DestinationUrl, request.Weight);
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/v1/links/{id}/variants/{result.Id}", result);
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

links.MapDelete("/{id:guid}/variants/{variantId:guid}", async (
    Guid id,
    Guid variantId,
    ClaimsPrincipal user,
    DeleteVariantHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        await handler.HandleAsync(new DeleteVariantCommand(variantId, tenantId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

// ── Authenticated: geo routes ────────────────────────────────────────────────

links.MapGet("/{id:guid}/geo-routes", async (
    Guid id,
    ClaimsPrincipal user,
    ListGeoRoutesHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    var result = await handler.HandleAsync(new ListGeoRoutesQuery(id, tenantId.Value), ct);
    return Results.Ok(result);
});

links.MapPost("/{id:guid}/geo-routes", async (
    Guid id,
    CreateGeoRouteRequest request,
    ClaimsPrincipal user,
    CreateGeoRouteHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        var command = new CreateGeoRouteCommand(id, tenantId.Value, request.CountryCode, request.DestinationUrl);
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/v1/links/{id}/geo-routes/{result.Id}", result);
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

links.MapDelete("/{id:guid}/geo-routes/{routeId:guid}", async (
    Guid id,
    Guid routeId,
    ClaimsPrincipal user,
    DeleteGeoRouteHandler handler,
    CancellationToken ct) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    try
    {
        await handler.HandleAsync(new DeleteGeoRouteCommand(routeId, tenantId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

// ── Authenticated: block / unblock link ──────────────────────────────────────

links.MapPost("/{id:guid}/block", async (
    Guid id,
    ClaimsPrincipal user,
    BlockLinkHandler handler,
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
        await handler.HandleAsync(new BlockLinkCommand(id, tenantId.Value, userId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

links.MapPost("/{id:guid}/unblock", async (
    Guid id,
    ClaimsPrincipal user,
    UnblockLinkHandler handler,
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
        await handler.HandleAsync(new UnblockLinkCommand(id, tenantId.Value, userId.Value), ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException ex)
    {
        var status = ex.Message.Contains("not found") ? StatusCodes.Status404NotFound : StatusCodes.Status422UnprocessableEntity;
        return Results.Problem(ex.Message, statusCode: status);
    }
});

// ── Authenticated: QR code ───────────────────────────────────────────────────

links.MapGet("/{id:guid}/qr", async (
    Guid id,
    ClaimsPrincipal user,
    GetLinkHandler getLinkHandler,
    IDomainRepository domainRepository,
    string format = "png",
    int size = 10,
    CancellationToken ct = default) =>
{
    var tenantId = ResolveTenantId(user);
    if (tenantId is null)
    {
        return Results.Forbid();
    }

    var link = await getLinkHandler.HandleAsync(new GetLinkQuery(id, tenantId.Value), ct);
    if (link is null)
    {
        return Results.NotFound();
    }

    var domain = await domainRepository.GetByIdAsync(link.DomainId, tenantId.Value, ct);
    if (domain is null)
    {
        return Results.NotFound();
    }

    var shortUrl = $"https://{domain.Host}/{link.ShortCode}";
    var clampedSize = Math.Clamp(size, 1, 20);

    using var qrGenerator = new QRCodeGenerator();
    var qrData = qrGenerator.CreateQrCode(shortUrl, QRCodeGenerator.ECCLevel.Q);

    if (format.Equals("svg", StringComparison.OrdinalIgnoreCase))
    {
        using var svgCode = new SvgQRCode(qrData);
        var svg = svgCode.GetGraphic(clampedSize);
        return Results.Content(svg, "image/svg+xml");
    }

    using var pngCode = new PngByteQRCode(qrData);
    var png = pngCode.GetGraphic(clampedSize);
    return Results.File(png, "image/png");
});

// ── Public: abuse report ──────────────────────────────────────────────────────

app.MapPost("/api/v1/public/reports", async (
    CreateAbuseReportRequest request,
    HttpContext ctx,
    CreateAbuseReportHandler handler,
    CancellationToken ct) =>
{
    var normalizedHost = TenantDomain.NormalizeHost(ctx.Request.Host.Value ?? string.Empty);
    var reporterIp = ctx.Connection.RemoteIpAddress?.ToString();

    try
    {
        var command = new CreateAbuseReportCommand(request.ShortCode, normalizedHost, reporterIp, request.Reason);
        await handler.HandleAsync(command, ct);
        return Results.NoContent();
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
    }
}).RequireRateLimiting("public-reports");

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
