using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Infrastructure.Abuse;
using Shortener.Infrastructure.Analytics;
using Shortener.Infrastructure.ApiKeys;
using Shortener.Infrastructure.Caching;
using Shortener.Infrastructure.Captcha;
using Shortener.Infrastructure.Geo;
using Shortener.Infrastructure.Domains;
using Shortener.Infrastructure.Persistence;
using Shortener.Infrastructure.Persistence.Repositories;
using Shortener.Infrastructure.Seeding;
using Shortener.Infrastructure.ShortCodes;
using Shortener.Infrastructure.Http;
using Shortener.Infrastructure.Webhooks;

namespace Shortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database — read connection string lazily so WebApplicationFactory config overrides take effect
        services.AddDbContext<ShortenerDbContext>((sp, opts) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var connStr = cfg.GetSection("Database").Get<DatabaseOptions>()?.ConnectionString
                ?? throw new InvalidOperationException("Database configuration is missing.");
            opts.UseNpgsql(connStr, npgsql => npgsql.MigrationsAssembly("Shortener.Migrator"));
        });

        // Factory for repositories that run concurrent queries (e.g. Task.WhenAll)
        services.AddDbContextFactory<ShortenerDbContext>((sp, opts) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var connStr = cfg.GetSection("Database").Get<DatabaseOptions>()?.ConnectionString
                ?? throw new InvalidOperationException("Database configuration is missing.");
            opts.UseNpgsql(connStr, npgsql => npgsql.MigrationsAssembly("Shortener.Migrator"));
        }, ServiceLifetime.Scoped);

        services.AddScoped<IShortLinkRepository, ShortLinkRepository>();
        services.AddScoped<IDomainRepository, DomainRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IWebhookRepository, WebhookRepository>();
        services.AddScoped<IAbuseReportRepository, AbuseReportRepository>();
        services.AddScoped<ILinkVariantRepository, LinkVariantRepository>();
        services.AddScoped<IGeoRouteRepository, GeoRouteRepository>();
        services.AddScoped<ILinkStatsRepository, LinkStatsRepository>();
        services.AddScoped<IClickEventRepository, ClickEventRepository>();
        services.AddScoped<IApiKeyAuthenticator, ApiKeyAuthenticator>();
        services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();

        // URL blocklist (reads from AbuseOptions.BlockedHosts at startup)
        services.Configure<AbuseOptions>(opts =>
            configuration.GetSection(AbuseOptions.SectionName).Bind(opts));
        services.AddSingleton<IUrlBlocklist, ConfigurableUrlBlocklist>();

        // Redis — read connection string lazily so WebApplicationFactory config overrides take effect
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var connStr = cfg.GetSection(RedisOptions.SectionName).Get<RedisOptions>()?.ConnectionString
                ?? throw new InvalidOperationException("Redis configuration is missing.");
            return ConnectionMultiplexer.Connect(connStr);
        });
        services.AddSingleton<IRedirectCache, RedirectCache>();
        services.Configure<RateLimitOptions>(opts =>
            configuration.GetSection(RateLimitOptions.SectionName).Bind(opts));
        services.AddSingleton<IRedirectRateLimiter, RedisRedirectRateLimiter>();
        services.AddSingleton<IEdgeCacheInvalidator, NoOpEdgeCacheInvalidator>();
        services.AddScoped<ILinkCacheInvalidator, LinkCacheInvalidator>();

        // Short code + captcha
        services.AddSingleton<IShortCodeGenerator, Base62ShortCodeGenerator>();
        services.AddSingleton<ICaptchaVerifier, DisabledCaptchaVerifier>();

        // Geo routing
        services.Configure<GeoOptions>(opts =>
            configuration.GetSection(GeoOptions.SectionName).Bind(opts));
        services.AddSingleton<IGeoResolver, MaxMindGeoResolver>();

        // Domain verifier (HTTP challenge)
        services.AddSingleton<IDomainVerifier, HttpDomainVerifier>();

        // Single-tenant seeder (no-op in MultiTenant mode)
        services.AddHostedService<SingleTenantSeeder>();

        return services;
    }

    /// <summary>
    /// Registers the in-process analytics buffer + RabbitMQ publisher.
    /// Call this in Shortener.Redirect (publisher side).
    /// </summary>
    public static IServiceCollection AddAnalyticsPipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ClickEventOptions>(opts =>
            configuration.GetSection(ClickEventOptions.SectionName).Bind(opts));
        services.Configure<RabbitMqOptions>(opts =>
            configuration.GetSection(RabbitMqOptions.SectionName).Bind(opts));

        services.AddSingleton<ClickEventChannel>();
        services.AddSingleton<IClickEventBuffer>(sp => sp.GetRequiredService<ClickEventChannel>());
        services.AddHostedService<RabbitMqAnalyticsPublisher>();

        return services;
    }

    /// <summary>
    /// Registers the RabbitMQ consumer + ClickEventRepository.
    /// Call this in Shortener.Worker (consumer side).
    /// </summary>
    public static IServiceCollection AddAnalyticsConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ClickEventOptions>(opts =>
            configuration.GetSection(ClickEventOptions.SectionName).Bind(opts));
        services.Configure<RabbitMqOptions>(opts =>
            configuration.GetSection(RabbitMqOptions.SectionName).Bind(opts));
        services.Configure<DatabaseOptions>(opts =>
            configuration.GetSection("Database").Bind(opts));

        services.AddHttpContextAccessor();
        services.AddTransient<ForwardAuthorizationHandler>();

        var geoIpBaseUrl = configuration["GeoIp:BaseUrl"] ?? "https://sandbox-geoip.klogs.io";
        services.AddHttpClient("geoip", c =>
        {
            c.BaseAddress = new Uri(geoIpBaseUrl);
            c.Timeout = TimeSpan.FromSeconds(3);
        })
        .AddHttpMessageHandler<ForwardAuthorizationHandler>();

        services.AddHostedService<RabbitMqAnalyticsConsumer>();

        return services;
    }
}
