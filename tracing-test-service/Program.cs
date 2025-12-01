using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet(
    "/test",
    ([FromServices] ActivitySource activitySource, [FromServices] SomeWorker worker) =>
    {
        using var activity = activitySource.StartActivity("HelloWorld");
        activity?.SetTag("custom.tag", "hello-world");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.route", "/test");
        worker.DoSomeWork();

        return "Hello World!";
    });

app.Run();