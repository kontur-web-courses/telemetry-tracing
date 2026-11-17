using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public const string ServiceName = "ABOBA_3_SPIDOMETER_3.0";

    public static void AddOtelTelemetry(this WebApplicationBuilder builder)
    {
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        void ConfigureResource(ResourceBuilder rb)
        {
            rb.AddService(
            serviceName: ServiceName,
            serviceVersion: version,
            autoGenerateServiceInstanceId: true);
            rb.AddEnvironmentVariableDetector();
        }

        builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(ConfigureResource)
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
                .AddMeter(ServiceName)
                .AddConsoleExporter()
                .AddOtlpExporter();
        });

        var logResource = ResourceBuilder.CreateDefault();
        ConfigureResource(logResource);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(logResource);
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;
            logging.AddConsoleExporter();
            logging.AddOtlpExporter();
        });
    }
}