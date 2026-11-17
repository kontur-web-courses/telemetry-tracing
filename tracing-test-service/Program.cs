using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddOtelLogging();
builder.Services.AddOpenTelemetry().AddOtelTelemetry();
builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));
builder.Services.AddSingleton(new Meter("tracing_test_service"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<Meter>().CreateCounter<long>("hello_world_requests"));

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker,
    [FromServices] Counter<long> requestsCounter,
    ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.AddTag("hello", "world");
    activity?.AddTag("tag-name", "tag-value");
    worker.DoSomeWork();

    requestsCounter.Add(1, new KeyValuePair<string, object?>("http.route", "/"));

    logger.LogInformation(
        "Handled GET / with TraceId {TraceId} and SpanId {SpanId}",
        Activity.Current?.TraceId,
        Activity.Current?.SpanId);

    return "Hello World!";
});

app.Run();
