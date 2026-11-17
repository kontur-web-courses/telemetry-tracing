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
    activity?.AddTag("имя тега", "значение тега");
    worker.DoSomeWork();
    return "Hello World!!!";
});

app.Run();