using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().AddOtelTracing();
builder.Services.AddSingleton<SomeWorker>();
builder.Services.AddSingleton(new ActivitySource("tracing_test_service"));

var app = builder.Build();

app.MapGet("/", ([FromServices] ActivitySource activitySource,
                 [FromServices] SomeWorker someWorker,
                 [FromServices] ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("Hello World");
    activity?.SetTag("test", "real-test");
    someWorker.DoSomeWork();
    logger.LogInformation("Handled hello request");
    return "Hello World!";
});

app.Run();
