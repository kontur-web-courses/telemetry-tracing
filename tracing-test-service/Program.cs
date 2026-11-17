using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using tracing_test_service;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        const string serviceName = "aboba_project";
        builder.Services.AddOpenTelemetry().AddOtelTracing();
        builder.Services.AddSingleton<SomeWorker>();
        builder.Services.AddSingleton(new ActivitySource(serviceName));

        // var meter = new Meter(name: serviceName);
        // builder.Services.AddSingleton(meter);
        // builder.Services.AddSingleton(meter.CreateCounter<int>("requests_total", "count", "Total processed requests"));

        // builder.Logging.AddOpenTelemetry(logging =>
        // {
        //     logging.IncludeFormattedMessage = true;
        //     logging.IncludeScopes = true;
        // });


        // builder.Services.AddOpenTelemetry()
        //     .ConfigureResource(resource => resource.AddService(serviceName))
        //     .WithTracing(tracing =>
        //     {
        //         tracing
        //             .AddAspNetCoreInstrumentation()
        //             .AddHttpClientInstrumentation()
        //             .AddSource(serviceName);
        //     })
        //     .WithMetrics(metrics =>
        //     {
        //         metrics
        //             .AddAspNetCoreInstrumentation()
        //             .AddHttpClientInstrumentation()
        //             .AddMeter(serviceName);
        //     })
        //     .UseOtlpExporter();


        var app = builder.Build();

        app.MapGet("/", async (
            [FromServices] ActivitySource activitySource,
            [FromServices] SomeWorker worker,
            ILogger<Program> logger) =>
        {
            using var activity = activitySource.StartActivity("HelloWorld");

            activity?.SetTag("user.name", "student");

            worker.DoSomeWork();
            logger.LogInformation("Successfully handled request");
            await Task.Delay(5000);
            await PrivateMethodAsync(activitySource);

            return "Hello World!";
        });


        app.Run();


    }

    private static async Task PrivateMethodAsync([FromServices] ActivitySource activitySource)
    {
        using var activity = activitySource.StartActivity("HelloWorld2");
        activity?.SetTag("user.password", "54321");

        await Task.Delay(10000);
    }
}
