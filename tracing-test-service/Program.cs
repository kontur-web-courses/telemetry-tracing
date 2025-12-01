using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<SomeWorker>();

var app = builder.Build();

app.MapGet("/", (
    [FromServices] ActivitySource activitySource,
    [FromServices] SomeWorker worker) =>
{
    using var activity = activitySource.StartActivity("HelloWorld");
    
    activity?.AddTag("http.method", "GET");
    activity?.AddTag("http.route", "/");
    activity?.AddTag("custom.tag", "custom_value");
    activity?.AddTag("environment", app.Environment.EnvironmentName);
    activity?.AddTag("timestamp", DateTime.UtcNow.ToString("o"));
    
    try
    {
        worker.DoSomeWork();
        
        return "Hello World!";
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddTag("error.message", ex.Message);
        activity?.AddTag("error.stacktrace", ex.StackTrace);
        throw;
    }
});
app.Run();