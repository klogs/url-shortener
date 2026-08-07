# SKILLS.md

Recurring patterns for this repository. Read before implementing any feature.

---

## Handler Pattern (no MediatR)

Every use case is an explicit handler class injected via DI.

```csharp
// Application/Links/Commands/CreateLink/CreateLinkCommand.cs
public sealed record CreateLinkCommand(
    Guid TenantId,
    Guid DomainId,
    string DestinationUrl,
    string? Alias,
    DateTimeOffset? ExpiresAt);

// Application/Links/Commands/CreateLink/CreateLinkHandler.cs
public sealed class CreateLinkHandler(
    IShortLinkRepository links,
    IShortCodeGenerator codeGen,
    TimeProvider time)
{
    public async Task<CreateLinkResult> HandleAsync(
        CreateLinkCommand cmd,
        CancellationToken ct = default)
    {
        // ...
    }
}

// Registration in DI (Infrastructure or Api project)
services.AddScoped<CreateLinkHandler>();
```

Endpoint calls handler directly — no pipeline, no reflection, no bus.

---

## Repository Pattern

Domain-specific interfaces only. No generic `IRepository<T>`.

```csharp
// Application/Interfaces/IShortLinkRepository.cs
public interface IShortLinkRepository
{
    Task<ShortLink?> GetByHostAndCodeAsync(string normalizedHost, string shortCode, CancellationToken ct);
    Task<bool> AliasExistsAsync(Guid domainId, string normalizedAlias, CancellationToken ct);
    Task InsertAsync(ShortLink link, CancellationToken ct);
    Task<PagedResult<ShortLink>> SearchAsync(ShortLinkSearchQuery query, CancellationToken ct);
}
```

All queries targeting tenant-owned data **must** filter by `TenantId` inside the repository — never rely on the caller to add the filter.

---

## Multi-Tenant Resolution

Never trust a `TenantId` from the HTTP request body or query string.

```csharp
// Resolve tenant from the authenticated token claim
public static class ClaimsPrincipalExtensions
{
    public static Guid GetTenantId(this ClaimsPrincipalidentity)
        => Guid.Parse(identity.FindFirstValue("tenant_id")
           ?? throw new InvalidOperationException("tenant_id claim missing"));
}
```

Always pass resolved `TenantId` to handlers — handlers never read `HttpContext`.

---

## Mapping (no AutoMapper)

Explicit extension methods in the Application layer.

```csharp
// Application/Links/Mapping/ShortLinkMappingExtensions.cs
public static class ShortLinkMappingExtensions
{
    public static ShortLinkDto ToDto(this ShortLink link) => new(
        link.Id,
        link.ShortCode,
        link.DestinationUrl,
        link.Status,
        link.ExpiresAt);
}
```

One file per aggregate. No base mapper classes.

---

## Options Pattern

Every configurable subsystem has a typed options class.

```csharp
// Infrastructure/Configuration/CaptchaOptions.cs
public sealed class CaptchaOptions
{
    public const string SectionName = "Captcha";
    public string Provider { get; init; } = "Disabled";
    public string SiteKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}

// Registration
services.AddOptions<CaptchaOptions>()
        .BindConfiguration(CaptchaOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();
```

Secrets only via environment variables / secret manager — never in appsettings.json.

---

## CAPTCHA Abstraction

```csharp
// Application/Interfaces/ICaptchaVerifier.cs
public interface ICaptchaVerifier
{
    Task<bool> VerifyAsync(string token, string? clientIp, CancellationToken ct);
}

// Infrastructure/Captcha/TurnstileCaptchaVerifier.cs  — production
// Infrastructure/Captcha/DisabledCaptchaVerifier.cs   — development / test
```

Register by configuration:

```csharp
if (captchaOptions.Provider == "Turnstile")
    services.AddScoped<ICaptchaVerifier, TurnstileCaptchaVerifier>();
else
    services.AddScoped<ICaptchaVerifier, DisabledCaptchaVerifier>();
```

---

## TimeProvider Usage

Never use `DateTime.UtcNow` or `DateTimeOffset.UtcNow` directly.

```csharp
// Injected from DI — real implementation uses TimeProvider.System
public sealed class CreateLinkHandler(TimeProvider time, ...)
{
    var now = time.GetUtcNow();
}

// In tests
var fakeTime = new FakeTimeProvider();
fakeTime.SetUtcNow(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
```

Register in DI: `services.AddSingleton(TimeProvider.System);`

---

## Short Code Generation

```csharp
// Application/Interfaces/IShortCodeGenerator.cs
public interface IShortCodeGenerator
{
    string Generate(); // Returns 7-char Base62 string
}

// Domain/ShortCodes/Base62ShortCodeGenerator.cs
// Uses RandomNumberGenerator.GetBytes — no Sequential IDs
```

Collision retry is the caller's responsibility (max 3 attempts, then throw).

---

## Redirect Cache Key

```
sl:{normalized-host}:{short-code}
```

Normalization: lowercase, strip trailing slash, no scheme.

```csharp
public static string BuildRedirectCacheKey(string host, string shortCode)
    => $"sl:{host.ToLowerInvariant().TrimEnd('/')}:{shortCode}";
```

---

## Analytics Event Enqueue (non-blocking)

```csharp
// Never await a DB write or RabbitMQ publish on the redirect path.
// Enqueue to Channel and return immediately.
_channel.Writer.TryWrite(clickEvent); // fire-and-forget; drop on full
```

Dropped event count must be incremented in `analytics_events_dropped_total` metric.

---

## Error Response

Use `TypedResults.Problem` (Minimal APIs) or `IProblemDetailsService`.

```csharp
return TypedResults.Problem(
    title: "Alias already exists",
    statusCode: 409,
    extensions: new Dictionary<string, object?> { ["code"] = "SHORT_LINK_ALIAS_TAKEN" });
```

No raw string messages as response bodies.

---

## Endpoint Organization (Minimal APIs)

Group endpoints by feature, not by HTTP method.

```csharp
// Api/Endpoints/Links/LinkEndpoints.cs
public static class LinkEndpoints
{
    public static IEndpointRouteBuilder MapLinkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/links").RequireAuthorization();
        group.MapPost("/", CreateLinkEndpoint.Handle);
        group.MapGet("/", SearchLinksEndpoint.Handle);
        group.MapGet("/{id:guid}", GetLinkEndpoint.Handle);
        return app;
    }
}
```

---

## Architecture Test Pattern

```csharp
// Tests/Shortener.ArchitectureTests/LayerDependencyTests.cs
[Fact]
public void Domain_must_not_reference_any_other_project()
{
    var domain = typeof(ShortLink).Assembly;
    // assert no ref to Application, Infrastructure, Api, Redirect assemblies
}

[Fact]
public void Application_must_not_reference_Infrastructure()
{
    var app = typeof(CreateLinkHandler).Assembly;
    // assert no ref to Infrastructure assembly
}
```

---

## Outbox Message

```csharp
// Domain/Outbox/OutboxMessage.cs
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string EventType { get; init; } = default!;   // e.g. "LinkCreated"
    public string Payload { get; init; } = default!;     // JSON
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
}
```

Written in the same DB transaction as the domain change. Worker polls and publishes.

---

## Naming Conventions

| Thing | Convention | Example |
|---|---|---|
| Commands | `<Verb><Noun>Command` | `CreateLinkCommand` |
| Queries | `Get/Search<Noun>Query` | `GetLinkAnalyticsQuery` |
| Handlers | `<Verb><Noun>Handler` | `CreateLinkHandler` |
| Results | `<Verb><Noun>Result` | `CreateLinkResult` |
| Repository interfaces | `I<Noun>Repository` | `IShortLinkRepository` |
| Options classes | `<Subsystem>Options` | `CaptchaOptions` |
| Domain events | `<Noun><PastVerb>` | `LinkCreated`, `LinkExpired` |
