using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry().AddTelemetry();

builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

var meterFactory = app.Services.GetRequiredService<IMeterFactory>();
var meter = meterFactory.Create(OpenTelemetryExtensions.SourceName);
var countRequests = meter.CreateCounter<int>("requests.index.count");

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker,
    [FromServices] ILogger<Program> logger) =>  
{
    countRequests.Add(1);

    using var activity = activitySource.StartActivity("HelloWorld");

    activity?.AddTag("worker.status", "started");
    activity?.AddTag("test.tag", "my-value");

    logger.LogInformation("Запрос обработан. TraceId: {TraceId}", Activity.Current?.TraceId.ToString());

    worker.DoSomeWork();

    return "Hello World!";
});

app.Run();