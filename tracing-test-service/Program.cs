using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry().AddOtelTracing();

var activitySource = new ActivitySource("example-service-group6");
var meter = new Meter("example-service-group6");
var counter = meter.CreateCounter<int>("requestsCount");

builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton(activitySource);
builder.Services.AddSingleton(counter);

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource source,
    [FromServices] Counter<int> requestCounter,
    [FromServices] ILogger<Program> logger,
    [FromServices] SomeWorker worker) =>
{
    using var activity = source.StartActivity("HelloWorld");
    activity?.SetTag("serviceTag", "tagValue");

    requestCounter.Add(1);
    logger.LogInformation("HelloWorld передает привет в логи");

    worker.DoSomeWork();

    return "Hello World!";
});

app.Run();