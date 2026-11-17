using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<ActivitySource>(_ => new ActivitySource("ikebanana"));
builder.Services.AddSingleton<SomeWorker>();

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddSource("ikebanana")
            .AddOtlpExporter();
    });

var app = builder.Build();

app.MapGet("/", ([FromServices] ActivitySource activitySource, [FromServices] SomeWorker worker) =>
{
    worker.DoSomeWork();
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.AddTag("hello_span", "pupupu");
    return "Hello, World!";
});

app.Run();