using Shortener.Application.Options;
using Shortener.Infrastructure;
using Shortener.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<DatabaseOptions>(opts =>
    builder.Configuration.GetSection("Database").Bind(opts));

// Infrastructure (repositories needed by workers)
builder.Services.AddInfrastructure(builder.Configuration);

// Analytics consumer: RabbitMQ → ClickEventRepository → PostgreSQL
builder.Services.AddAnalyticsConsumer(builder.Configuration);

// Webhook delivery worker
builder.Services.AddHostedService<WebhookDeliveryWorker>();

var host = builder.Build();
host.Run();
