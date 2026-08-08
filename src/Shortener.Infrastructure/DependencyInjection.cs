using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Infrastructure.Persistence;
using Shortener.Infrastructure.Persistence.Repositories;

namespace Shortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection("Database").Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("Database configuration is missing.");

        services.AddDbContext<ShortenerDbContext>(opts =>
            opts.UseNpgsql(dbOptions.ConnectionString,
                npgsql => npgsql.MigrationsAssembly("Shortener.Migrator")));

        services.AddScoped<IShortLinkRepository, ShortLinkRepository>();
        services.AddScoped<IDomainRepository, DomainRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        return services;
    }
}
