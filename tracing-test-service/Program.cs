using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTelemetry();
builder.Services.AddSingleton(_ => new ActivitySource(OpenTelemetryExtensions.InstrumentationName));
builder.Services.AddSingleton(_ => new Meter(OpenTelemetryExtensions.InstrumentationName));
builder.Services.AddSingleton(services => services.GetRequiredService<Meter>().CreateCounter<long>(
    "hello_world.requests",
    unit: "{request}",
    description: "Number of successfully handled Hello World requests."));
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker,
    [FromServices] Counter<long> requestCounter,
    [FromServices] ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.AddTag("worker.name", nameof(SomeWorker));
    activity?.AddTag("worker.method", nameof(SomeWorker.DoSomeWork));

    worker.DoSomeWork();
    requestCounter.Add(1, new KeyValuePair<string, object?>("http.route", "/"));
    logger.LogInformation("Hello World request handled by {Worker}", nameof(SomeWorker));

    return "Hello World!";
});

app.Run();
