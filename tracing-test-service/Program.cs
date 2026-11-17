using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOpenTelemetry()
    .AddOtelTracing()
    .WithTracing();

builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", ([FromServices] ActivitySource activitySource, [FromServices] SomeWorker worker) =>
{
    worker.DoSomeWork();
    using var activity = activitySource.StartActivity("HelloWorld");
    activity?.AddTag("hello_span", "pupupu");
    return "Hello, World!";
});

app.Run();