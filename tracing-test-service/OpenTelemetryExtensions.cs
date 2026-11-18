using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public static OpenTelemetryBuilder AddOtelTelemetry(this OpenTelemetryBuilder openTelemetryBuilder) => openTelemetryBuilder
        .ConfigureResource(rb =>
        {
            var name = typeof(Program).Assembly.GetName();
            rb.AddService(
                serviceName: "tracing-test-service",
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
                .AddConsoleExporter()
                .AddOtlpExporter();
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("tracing_test_service")
                .AddOtlpExporter();
        })
        .WithLogging(
            logging => logging.AddOtlpExporter(),
            options => options.IncludeFormattedMessage = true);
}
