using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));
builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton<Metrics>();

builder.Services.AddOpenTelemetry()
    .AddOtelTracing()
    .WithMetrics(metrics => metrics.AddMeter(Metrics.MeterName))
    .WithLogging()
    .UseOtlpExporter();

var app = builder.Build();

app.MapGet("/", ([FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker,
    [FromServices] Metrics metrics,
    [FromServices] ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.AddTag("demo.hello", "world");
    metrics.HelloRequestHandled();
    logger.LogInformation("Handled hello request");
    worker.DoSomeWork();
    return "Hello World!";
});

app.Run();
