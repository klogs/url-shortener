using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shortener.Infrastructure.Persistence;

namespace Shortener.Migrator;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShortenerDbContext>
{
    public ShortenerDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            config.GetConnectionString("Database")
            ?? config["Database:ConnectionString"]
            ?? "Server=172.16.100.209;Port=5432;Database=shortener;User Id=postgres;Password=Zt9kkPadiEnfVgZDxYxy3g;Pooling=true";

        var opts = new DbContextOptionsBuilder<ShortenerDbContext>()
            .UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly("Shortener.Migrator"))
            .Options;

        return new ShortenerDbContext(opts);
    }
}
