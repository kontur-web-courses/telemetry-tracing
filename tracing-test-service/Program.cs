using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTelemetry();
builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));
builder.Services.AddSingleton(new Meter("tracing_test_service"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<Meter>()
    .CreateCounter<long>("komarov.requests", description: "Сколько раз вызвали /doKomarov"));
var app = builder.Build();


app.MapGet("/", () => "Hello World!");
app.MapGet("/doKomarov", (
    [FromServices] SomeWorker worker,
    [FromServices] ActivitySource activitySource,
    [FromServices] Counter<long> requestsCounter,
    [FromServices] ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("Komarov");
    if (activity is { IsAllDataRequested: true })
    {
        activity.SetTag("http.method", "Get");
        activity.SetTag("Komarov", "WW");
    }

    requestsCounter.Add(1);
    logger.LogInformation("Komarov: обработан запрос от {Student}", "mefody42");

    worker.DoSomeWork();
});

app.Run();
