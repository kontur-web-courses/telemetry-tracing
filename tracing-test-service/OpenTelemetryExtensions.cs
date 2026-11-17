using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public const string ServiceName = "tracing_test_service";
    public const string MeterName = "tracing_test_service";

    public static OpenTelemetryBuilder AddOtelTelemetry(this OpenTelemetryBuilder openTelemetryBuilder) => openTelemetryBuilder
        .ConfigureResource(rb =>
        {
            var name = typeof(Program).Assembly.GetName();
            var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new InvalidOperationException("OTEL_SERVICE_NAME environment variable is not set.");
            }
            rb.AddService(
                serviceName: serviceName,
                serviceVersion: name.Version!.ToString(),
                autoGenerateServiceInstanceId: true);
            rb.AddEnvironmentVariableDetector();
        })
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(ServiceName)
                .AddConsoleExporter()
                .AddOtlpExporter(otlp =>
                {
                    otlp.ExportProcessorType = ExportProcessorType.Simple;
                });
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter(MeterName)
                .AddOtlpExporter();
        })
        .WithLogging(logging =>
        {
            logging.AddOtlpExporter(otlp =>
            {
                otlp.ExportProcessorType = ExportProcessorType.Simple;
            });
        });
}
