using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public const string SourceName = "tracing_testing_serivce";

    public static IOpenTelemetryBuilder AddTelemetry(this OpenTelemetryBuilder openTelemetryBuilder)
    {
        openTelemetryBuilder.Services.AddSingleton(new ActivitySource(SourceName));
        
        return openTelemetryBuilder
            .ConfigureResource(rb => rb.AddService(
                serviceName: "TestService123",
                serviceVersion: typeof(Program).Assembly.GetName().Version!.ToString(),
                autoGenerateServiceInstanceId: true))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(SourceName))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter(SourceName))
            .WithLogging()
            .UseOtlpExporter();
    }
}