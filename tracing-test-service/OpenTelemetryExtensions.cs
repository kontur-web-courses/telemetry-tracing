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
    public const string ActivitySourceName = "tracing_test_service";
    public const string MeterName = "tracing_test_service";

    public static IOpenTelemetryBuilder AddOtelTracing(this IOpenTelemetryBuilder openTelemetryBuilder)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")))
        {
            Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4318");
        }
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL")))
        {
            Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf");
        }

        return openTelemetryBuilder
            .ConfigureResource(rb =>
            {
                var name = typeof(Program).Assembly.GetName();
                var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "ke-conf-otel-mc-example-service";
                rb.AddService(
                    serviceName: serviceName,
                    serviceVersion: name.Version?.ToString() ?? "1.0.0",
                    autoGenerateServiceInstanceId: true);
                rb.AddEnvironmentVariableDetector();
            })
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(ActivitySourceName)
                .AddConsoleExporter();
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter(MeterName)
                .AddConsoleExporter();
        })
        .WithLogging(logging =>
        {
            logging.AddConsoleExporter();
        })
        .UseOtlpExporter();
    }
}