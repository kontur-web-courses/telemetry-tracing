using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using tracing_test_service;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOpenTelemetry();
builder.AddOtelTelemetry();
builder.Services.AddSingleton<SomeWorker>();

var activitySource = new ActivitySource("ABOBA_3_SPIDOMETER_3.0");
builder.Services.AddSingleton(activitySource);

var meter = new Meter(OpenTelemetryExtensions.ServiceName);
var requestCounter = meter.CreateCounter<long>(
    name: "aboba_hello_requests_total",
    description: "Количество запросов к корневому эндпоинту"
);

builder.Services.AddSingleton(meter);
builder.Services.AddSingleton(requestCounter);

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource source,
    [FromServices] Counter<long> counter,
    [FromServices] SomeWorker worker,
    [FromServices] ILogger<Program> logger) =>
{
    using var activity = source.StartActivity("GOODWorld");

    activity?.AddTag("aboba.tag", "spidometer");
    activity?.AddTag("worker.name", nameof(SomeWorker));

    counter.Add(1,
        new("endpoint", "/"),
        new("worker", nameof(SomeWorker))
    );

    logger.LogInformation("Обработка запроса для {Worker}, TraceId: {TraceId}",
        nameof(SomeWorker),
        activity?.TraceId.ToString()
    );

    worker.DoSomeWork();

    return "Hello World!";
});

app.Run();