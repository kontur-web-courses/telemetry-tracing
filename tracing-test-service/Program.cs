using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));
var app = builder.Build();


app.MapGet("/", () => "Hello World!");
app.MapGet("/doKomarov", ([FromServices] SomeWorker worker, [FromServices] ActivitySource activitySource) =>
{
    using var activity = activitySource.StartActivity("Komarov");
    if (activity is { IsAllDataRequested: true })
    {
        activity.SetTag("http.method", "Get");
        activity.SetTag("Komarov", "WW");
    }
    worker.DoSomeWork();
});

app.Run();