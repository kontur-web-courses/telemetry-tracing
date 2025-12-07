using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();


app.MapGet("/", ([FromServices] ActivitySource activitySource, [FromServices] SomeWorker worker) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");

    activity?.SetTag("http.method", "GET");
    activity?.SetTag("worker.status", "started");

    worker.DoSomeWork();
    
    activity?.SetTag("worker.status", "finished");

    return "Hello World!";
});

app.Run();