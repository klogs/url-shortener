using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Infrastructure.Analytics;
using Shortener.Infrastructure.Caching;
using Shortener.Infrastructure.Captcha;
using Shortener.Infrastructure.Persistence;
using Shortener.Infrastructure.Persistence.Repositories;
using Shortener.Infrastructure.Seeding;
using Shortener.Infrastructure.ShortCodes;

namespace Shortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var dbOptions = configuration.GetSection("Database").Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("Database configuration is missing.");

        services.AddDbContext<ShortenerDbContext>(opts =>
            opts.UseNpgsql(dbOptions.ConnectionString,
                npgsql => npgsql.MigrationsAssembly("Shortener.Migrator")));

        services.AddScoped<IShortLinkRepository, ShortLinkRepository>();
        services.AddScoped<IDomainRepository, DomainRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        // Redis
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? throw new InvalidOperationException("Redis configuration is missing.");

        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
        services.AddSingleton<IRedirectCache, RedirectCache>();

        // Short code + captcha
        services.AddSingleton<IShortCodeGenerator, Base62ShortCodeGenerator>();
        services.AddSingleton<ICaptchaVerifier, DisabledCaptchaVerifier>();

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
}
