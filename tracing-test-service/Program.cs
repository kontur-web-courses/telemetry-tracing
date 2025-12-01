using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracing_test_service;

var builder = WebApplication.CreateBuilder(args);

builder.AddTelemetry();
builder.Services.AddScoped<SomeWorker>();

var app = builder.Build();

app.MapGet("/hello",
    ([FromServices] ActivitySource activitySource,
     [FromServices] SomeWorker worker) =>
{
    using var activity = activitySource.StartActivity("MyActivity");
    activity?.SetTag("endpoint", "/test");
    activity?.SetTag("best_tag", "yes");
    activity?.SetTag("worker", nameof(SomeWorker));

    return Results.Ok("Hello!");
});

app.Run();
