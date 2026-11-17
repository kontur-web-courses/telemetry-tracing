using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public static OpenTelemetryBuilder AddOtelTracing(this OpenTelemetryBuilder openTelemetryBuilder) => (OpenTelemetryBuilder)openTelemetryBuilder
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("example-service-group6")
                .AddConsoleExporter();
        })
        .ConfigureResource(rb =>
        {
            var name = typeof(Program).Assembly.GetName();
            rb.AddService(
                serviceName: "example-service-group6",
                serviceVersion: name.Version!.ToString(),
                autoGenerateServiceInstanceId: true);
            rb.AddEnvironmentVariableDetector();
        })
        .WithLogging(logging => logging
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("example-service-group6")
            .AddConsoleExporter())
        .UseOtlpExporter();
}