using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton(new ActivitySource(OpenTelemetryExtensions.ActivitySourceName));
builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton<Metrics>();

var app = builder.Build();

app.MapGet("/", ([FromServices] ActivitySource activitySource,
                 [FromServices] SomeWorker worker,
                 [FromServices] Metrics metrics,
                 [FromServices] ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.AddTag("custom.tag", "custom_value");

    var traceId = activity?.TraceId.ToString() ?? Activity.Current?.TraceId.ToString() ?? "none";
    logger.LogInformation("Processing hello world request. Activity TraceId: {TraceId}", traceId);

    worker.SomeWork();

    sw.Stop();
    metrics.RecordRequest(sw.Elapsed);

    return $"Hello World! (TraceId: {traceId})";
});

app.Run();