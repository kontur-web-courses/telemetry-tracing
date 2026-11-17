using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public static IOpenTelemetryBuilder AddOtelTracing(this IOpenTelemetryBuilder openTelemetryBuilder) => openTelemetryBuilder
        .ConfigureResource(rb =>
        {
            var name = typeof(Program).Assembly.GetName();
            rb.AddService(
                serviceName: "Rent-Control",
                serviceVersion: name.Version!.ToString(),
                autoGenerateServiceInstanceId: true);
            rb.AddEnvironmentVariableDetector();
        })
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("tracing_test_service")
                .AddConsoleExporter();
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation();
        })
        .WithLogging()
        .UseOtlpExporter();
}
