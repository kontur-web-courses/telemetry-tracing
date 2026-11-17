using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton(new ActivitySource(Telemetry.SourceName));
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", ([FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker, ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.SetTag("greeting.message", "Hello World!");
    worker.DoSomeWork();
    Telemetry.Requests.Add(1);
    logger.LogInformation("Hello World request handled");
    return "Hello World!";
});

app.Run();

public static class Telemetry
{
    public const string SourceName = "tracing_test_service";
    public static readonly Meter Meter = new(SourceName);
    public static readonly Counter<long> Requests = Meter.CreateCounter<long>("hello.requests", description: "Number of Hello World requests handled");
}
