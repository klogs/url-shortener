using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Shortener.Application.Observability;
using Shortener.Application.Options;
using Shortener.Infrastructure;
using Shortener.Worker;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "shortener-worker")
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, cfg) => cfg
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "shortener-worker")
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter()));

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<DatabaseOptions>(opts =>
    builder.Configuration.GetSection("Database").Bind(opts));

// Infrastructure (repositories needed by workers)
builder.Services.AddInfrastructure(builder.Configuration);

// Analytics consumer: RabbitMQ → ClickEventRepository → PostgreSQL
builder.Services.AddAnalyticsConsumer(builder.Configuration);

// OpenTelemetry
builder.Services.AddSingleton<ShortenerMetrics>();

var otelEndpoint = builder.Configuration["Otel:Endpoint"];
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("shortener-worker", serviceVersion: "1.0.0"))
    .WithTracing(tracing =>
    {
        if (!string.IsNullOrEmpty(otelEndpoint))
        {
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
        }
    });

// Webhook delivery worker
builder.Services.AddHostedService<WebhookDeliveryWorker>();

// Abuse: auto-block links above report threshold
builder.Services.Configure<AbuseOptions>(opts =>
    builder.Configuration.GetSection(AbuseOptions.SectionName).Bind(opts));
builder.Services.AddHostedService<AbuseReportSweepWorker>();

// Expiration: notify tenants of links expiring within 7 days
builder.Services.AddHostedService<LinkExpirationSweepWorker>();

var host = builder.Build();
host.Run();
