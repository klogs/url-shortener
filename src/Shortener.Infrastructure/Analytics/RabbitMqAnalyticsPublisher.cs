using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shortener.Application.Options;
using Shortener.Domain.Events;

namespace Shortener.Infrastructure.Analytics;

public sealed class RabbitMqAnalyticsPublisher(
    ClickEventChannel buffer,
    IOptions<RabbitMqOptions> rabbitOpts,
    IOptions<ClickEventOptions> analyticsOpts,
    ILogger<RabbitMqAnalyticsPublisher> logger) : BackgroundService
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

                logger.LogInformation("Analytics publisher connected to RabbitMQ, draining channel.");

                await foreach (var evt in buffer.Reader.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        await PublishAsync(channel, evt, stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogWarning(ex, "Failed to publish click event {LinkId}.", evt.LinkId);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RabbitMQ analytics publisher connection lost. Retrying in 5s.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task PublishAsync(IChannel channel, ClickEvent evt, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json",
        };

        await channel.BasicPublishAsync(
            exchange: _analytics.Exchange,
            routingKey: _analytics.RoutingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }
}
