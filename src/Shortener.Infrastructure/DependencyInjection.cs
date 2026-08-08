using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Infrastructure.Caching;
using Shortener.Infrastructure.Captcha;
using Shortener.Infrastructure.Persistence;
using Shortener.Infrastructure.Persistence.Repositories;
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

        return services;
    }
}
