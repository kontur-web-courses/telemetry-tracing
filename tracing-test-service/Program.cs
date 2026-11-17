using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry()
    .AddOtelTracing()
    .WithLogging()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter()
        .AddMeter(nameof(Metrics)))
    .ConfigureResource(r => r.AddService("just-telemetry"))
    .UseOtlpExporter();
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", ([FromServices] SomeWorker worker, [FromServices] ActivitySource activitySource, [FromServices] ILogger<Program> logger, 
    [FromServices] IMeterFactory meterFactory) =>
{
    var metrics = new Metrics(meterFactory);
    using var activity = activitySource.StartActivity("HelloWorld");
    worker.DoSomeWork();
    activity.AddTag("newTag", "home");
    
    logger.LogInformation("Some logs for some services");
    var sw = Stopwatch.StartNew();
    for (var i = 0; i < new Random().Next(0, 100); i++)
    {
        Console.Write("1");
    }
    sw.Stop();
    metrics.RequestToIndex(sw.Elapsed);
    
    logger.LogInformation("End work with logs");
});

app.Run();
