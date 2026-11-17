using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<ActivitySource>(_ => new ActivitySource("Aboba"));
builder.Services.AddSingleton<SomeWorker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddSource("Aboba")
            .AddOtlpExporter(options =>
            {
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
    });

var app = builder.Build();

app.MapGet("/", ([FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");

    activity?.AddTag("Абоба", "Абобус");

    worker.DoSomeWork();

    return "Hello World!";
});

app.Run();