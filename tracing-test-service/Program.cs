using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
using var meter = new Meter("tracing_test_service");
var requestsCount = meter.CreateCounter<int>("requests.index.count");
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r =>
        r.AddService("kalendar"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("tracing_test_service"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("tracing_test_service"))
    .WithLogging()
    .UseOtlpExporter();
builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));



var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker,
    [FromServices] ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("TestSpan");
    activity?.SetTag("TestTag", "TestValue");

    var sw = Stopwatch.StartNew();
    worker.DoSomeWork();
    sw.Stop();

    requestsCount.Add(1);
    logger.LogInformation("Обработан запрос к главной странице");

    return "TestSpan";
});

app.Run();
