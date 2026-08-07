var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// /{shortCode} endpoint will be wired here in Phase 1

app.Run();

// Marker for integration tests
public partial class Program;
