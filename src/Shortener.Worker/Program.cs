using Shortener.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<PlaceholderWorker>();

var host = builder.Build();
host.Run();
