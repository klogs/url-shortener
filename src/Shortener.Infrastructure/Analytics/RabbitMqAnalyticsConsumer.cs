using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Domain.Events;

namespace Shortener.Infrastructure.Analytics;

public sealed class RabbitMqAnalyticsConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> rabbitOpts,
    IOptions<ClickEventOptions> analyticsOpts,
    IHttpClientFactory httpClientFactory,
    ILogger<RabbitMqAnalyticsConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions _rabbit = rabbitOpts.Value;
    private readonly ClickEventOptions _analytics = analyticsOpts.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbit.Host,
            Port = _rabbit.Port,
            VirtualHost = _rabbit.VirtualHost,
            UserName = _rabbit.Username,
            Password = _rabbit.Password,
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.ExchangeDeclareAsync(
                    _analytics.Exchange,
                    ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    _analytics.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    _analytics.Queue,
                    _analytics.Exchange,
                    _analytics.RoutingKey,
                    cancellationToken: stoppingToken);

                await channel.BasicQosAsync(0, prefetchCount: 50, global: false, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.Span);
                        var evt = JsonSerializer.Deserialize<ClickEvent>(json);
                        if (evt is not null)
                        {
                            // Enrich with country if IP is available and country not already set
                            if (evt.Country is null && !string.IsNullOrEmpty(evt.RemoteIp))
                            {
                                var country = await ResolveCountryAsync(evt.RemoteIp, stoppingToken);
                                if (country is not null)
                                {
                                    evt = evt with { Country = country };
                                }
                            }

                            using var scope = scopeFactory.CreateScope();
                            var repo = scope.ServiceProvider.GetRequiredService<IClickEventRepository>();
                            await repo.InsertAsync(evt, stoppingToken);
                        }

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogError(ex, "Failed to process click event. Nacking.");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    _analytics.Queue,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                logger.LogInformation("Analytics consumer started on queue {Queue}.", _analytics.Queue);

                // Keep alive until cancelled or connection drops
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Analytics consumer connection lost. Retrying in 5s.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task<string?> ResolveCountryAsync(string ip, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("geoip");
            var response = await client.GetFromJsonAsync<GeoIpResponse>($"/{Uri.EscapeDataString(ip)}", ct);
            return response?.CountryCode is { Length: 2 } cc ? cc : null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogDebug(ex, "GeoIP lookup failed for IP {Ip}.", ip);
            return null;
        }
    }

    private sealed record GeoIpResponse(string? CountryCode);
}
