using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(
    new ActivitySource("tracing_test_service")
);

builder.Services.AddSingleton<SomeWorker>();

builder.Services.AddOpenTelemetry()
    .AddOtelTracing();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/hello",
    (
        [FromServices] ActivitySource activitySource,
        [FromServices] SomeWorker worker
    ) =>
    {
        using var activity =
            activitySource.StartActivity("HelloWorld");
        activity?.SetTag(
            "custom.tag",
            "Hello OpenTelemetry"
        );

        activity?.SetTag(
            "operation.type",
            "demo"
        );
        worker.DoSomeWork();

        return Results.Ok("Hello World");
    });

app.Run();