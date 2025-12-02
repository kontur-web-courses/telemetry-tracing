using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using tracing_test_service;
using OpenTelemetry.Exporter.Jaeger;

var builder = WebApplication.CreateBuilder(args);

var activitySource = new ActivitySource("TestService");
builder.Services.AddSingleton(activitySource);

builder.Services.AddOpenTelemetry().AddOtelTracing();

builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");
app.MapGet("/",
    (
        [FromServices] ActivitySource source,
        [FromServices] SomeWorker worker) =>
    {
        using var activity = source.StartActivity("HelloWorld");

        activity?.AddTag("Custom", "Hello");
        activity?.AddTag("Time", DateTime.Now);

        worker.DoSomeWork();

        return "ok";
    });

app.Run();