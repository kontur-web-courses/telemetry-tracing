using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtel(builder.Configuration);
builder.Services.AddSingleton(Telemetry.ActivitySource);
builder.Services.AddSingleton(Telemetry.RequestsCounter);
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker,
    [FromServices] Counter<long> requestsCounter,
    [FromServices] ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("tracing_test_service.HelloWorld");

    // Кастомный спан. Благодаря AddSource(...) activity не будет null.
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.AddTag("custom.tag", "custom-value");
    activity?.AddTag("endpoint", "/");
    activity?.AddTag("correlation.id", activity.TraceId.ToString());

    worker.DoSomeWork();

    // Кастомная метрика.
    requestsCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/"));

    // Лог, связанный с трассировкой: в записи будет тот же TraceId/SpanId.
    logger.LogInformation(
        "Обработан запрос {Endpoint}, TraceId: {TraceId}, SpanId: {SpanId}",
        "/",
        Activity.Current?.TraceId,
        Activity.Current?.SpanId);

    return "Hello World!";
});

app.Run();
