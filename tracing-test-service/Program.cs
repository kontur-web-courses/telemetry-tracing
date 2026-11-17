using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("tracing_test_service")
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("tracing_test_service"))
    .WithLogging()
    .UseOtlpExporter();

builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));

var meter = new Meter("tracing_test_service");
var requestsCounter = meter.CreateCounter<long>("hello_requests_total");

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker,
    [FromServices] ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.AddTag("my.tag", "my value");
    activity?.AddTag("user.name", "Test");

    requestsCounter.Add(1);
    logger.LogInformation("Handling hello request, traceId={TraceId}", Activity.Current?.TraceId);

    worker.DoSomeWork();

    return "Hello World!";
});

app.Run();