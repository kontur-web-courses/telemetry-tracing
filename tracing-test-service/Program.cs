using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new ActivitySource(OpenTelemetryExtensions.SourceName));
builder.Services.AddSingleton(new Meter(OpenTelemetryExtensions.SourceName));

// Шаг 3.2 (общий сервис). Для шагов 1-3 (Jaeger) замени на .AddOtelTracing()
builder.Services.AddOpenTelemetry().AddOtelAll();

builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] Meter meter,
    [FromServices] SomeWorker worker,
    [FromServices] ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.SetTag("my.tag", "my-value");

    meter.CreateCounter<long>("hello_requests_total").Add(1);

    worker.DoSomeWork();

    logger.LogInformation("Hello handled, traceId={TraceId}", Activity.Current?.TraceId.ToString());
    return "Hello World!";
});

app.Run();