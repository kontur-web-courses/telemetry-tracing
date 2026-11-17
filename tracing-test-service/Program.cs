using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "HelloTelemetries";

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
        .AddConsoleExporter();
});

builder
    .Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithLogging(logging => logging.AddConsoleExporter())
    .WithTracing(tracing =>
        tracing.AddAspNetCoreInstrumentation().AddConsoleExporter().AddHttpClientInstrumentation()
    )
    .WithMetrics(metrics =>
        metrics.AddAspNetCoreInstrumentation().AddConsoleExporter().AddHttpClientInstrumentation()
    )
    .UseOtlpExporter();

builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.SetTag("my.tag", "my value");

    worker.DoSomeWork();

    return "Hello World!";
});

app.Run();