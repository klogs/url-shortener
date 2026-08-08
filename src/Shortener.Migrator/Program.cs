using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shortener.Infrastructure.Persistence;

var host = Host.CreateApplicationBuilder(args);

host.Configuration.AddJsonFile("appsettings.json", optional: true);
host.Configuration.AddEnvironmentVariables();

host.Services.AddDbContext<ShortenerDbContext>(opts =>
    opts.UseNpgsql(
        host.Configuration.GetConnectionString("Database")
        ?? host.Configuration["Database:ConnectionString"]
        ?? throw new InvalidOperationException(
            "Set ConnectionStrings:Database or Database:ConnectionString before running migrations."),
        npgsql => npgsql.MigrationsAssembly("Shortener.Migrator")));

var app = host.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ShortenerDbContext>();

Console.WriteLine("Applying migrations...");
await db.Database.MigrateAsync();
Console.WriteLine("Done.");
