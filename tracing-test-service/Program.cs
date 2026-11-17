using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry().AddOtelTelemetry();
builder.Services.AddSingleton(new ActivitySource(OpenTelemetryExtensions.ServiceName));
builder.Services.AddSingleton(_ => new Meter(OpenTelemetryExtensions.MeterName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<Meter>().CreateCounter<long>("hello.requests"));
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] Counter<long> requestCounter,
    [FromServices] SomeWorker worker,
    [FromServices] ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");

    activity?.AddTag("custom.tag", "аоаоаоа");

    requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/"));

    worker.DoSomeWork();

    logger.LogInformation(
        "Handled GET / traceId={TraceId} spanId={SpanId}",
        activity?.TraceId.ToString(),
        activity?.SpanId.ToString());

    return "Hello World!";
});

app.Run();
