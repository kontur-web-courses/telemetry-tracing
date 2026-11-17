using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", ([FromServices]ActivitySource activitySource, [FromServices] SomeWorker someWorker) =>
{
    using var activity = activitySource.StartActivity("Hello World");
    activity?.SetTag("test", "real-test");
    someWorker.DoSomeWork();
    return "Hello World!";
});

app.Run();