using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shortener.Application.Interfaces;
using Shortener.IntegrationTests.Auth;
using Shortener.IntegrationTests.Infrastructure;

namespace Shortener.IntegrationTests;

public sealed class ApiIntegrationFactory : WebApplicationFactory<Shortener.Api.ApiMarker>
{
    private readonly IntegrationFixture? _fixture;

    public ApiIntegrationFactory(IntegrationFixture? fixture = null)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Auth:Authority"] = "",
                ["RabbitMq:Host"] = "localhost",
                ["RabbitMq:Port"] = "5672",
                ["RabbitMq:Username"] = "guest",
                ["RabbitMq:Password"] = "guest",
            };

            if (_fixture is not null)
            {
                overrides["Database:ConnectionString"] = _fixture.PostgresConnectionString;
                overrides["Redis:ConnectionString"] = _fixture.RedisConnectionString;
            }

            config.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            // Replace analytics buffer with no-op so RabbitMQ is never contacted
            services.AddSingleton<IClickEventBuffer, NullClickEventBuffer>();

            // Replace auth with test handler
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            services.AddAuthorization(opts =>
            {
                var testPolicy = new AuthorizationPolicyBuilder(TestAuthHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .Build();
                opts.DefaultPolicy = testPolicy;
                opts.AddPolicy("ApiKeyLinksWrite", testPolicy);
            });
        });
    }
}
