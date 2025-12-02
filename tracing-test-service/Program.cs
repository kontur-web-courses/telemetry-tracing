using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", ([FromServices] ActivitySource activitySource, [FromServices] SomeWorker worker) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");

    activity?.SetTag("custom_shaslichok", "kurganmash");

    worker.DoSomeWork();

    return "Hello World!";
});

app.Run();