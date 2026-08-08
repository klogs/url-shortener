using Shortener.Application.Options;
using Shortener.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<DatabaseOptions>(opts =>
    builder.Configuration.GetSection("Database").Bind(opts));

// Analytics consumer: RabbitMQ → ClickEventRepository → PostgreSQL
builder.Services.AddAnalyticsConsumer(builder.Configuration);

var host = builder.Build();
host.Run();
