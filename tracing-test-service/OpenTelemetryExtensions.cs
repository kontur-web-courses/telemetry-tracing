using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public static OpenTelemetryBuilder AddOtelTracing(this OpenTelemetryBuilder openTelemetryBuilder) =>
        openTelemetryBuilder
            .ConfigureResource(rb =>
            {
                var name = typeof(Program).Assembly.GetName();
                rb.AddService(
                    serviceName: Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME")
                                 ?? "ke-conf-otel-mc-example-service",
                    serviceVersion: name.Version!.ToString(),
                    autoGenerateServiceInstanceId: true);
                rb.AddEnvironmentVariableDetector();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddConsoleExporter()
                    .AddSource("tracing_test_service")
                    .AddHttpClientInstrumentation();
            });
}
