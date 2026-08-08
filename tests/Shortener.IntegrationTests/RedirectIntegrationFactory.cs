using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shortener.Application.Interfaces;
using Shortener.IntegrationTests.Infrastructure;

namespace Shortener.IntegrationTests;

public sealed class RedirectIntegrationFactory : WebApplicationFactory<Shortener.Redirect.RedirectMarker>
{
    private readonly IntegrationFixture _fixture;

    public RedirectIntegrationFactory(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = _fixture.PostgresConnectionString,
                ["Redis:ConnectionString"] = _fixture.RedisConnectionString,
                ["RabbitMq:Host"] = "localhost",
                ["RabbitMq:Port"] = "5672",
                ["RabbitMq:Username"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["Geo:DatabasePath"] = "",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IClickEventBuffer, NullClickEventBuffer>();
        });
    }
}
