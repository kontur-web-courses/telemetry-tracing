using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public static OpenTelemetryBuilder AddOtelTelemetry(this OpenTelemetryBuilder openTelemetryBuilder) => openTelemetryBuilder
        .ConfigureResource(ConfigureResource)
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("tracing_test_service")
                .AddConsoleExporter()
                .AddOtlpExporter(ConfigureOtlp);
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("tracing_test_service")
                .AddOtlpExporter(ConfigureOtlp);
        });

    public static ILoggingBuilder AddOtelLogging(this ILoggingBuilder logging) => logging.AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
        options.SetResourceBuilder(CreateResourceBuilder());
        options.AddOtlpExporter(ConfigureOtlp);
    });

    private static ResourceBuilder CreateResourceBuilder()
    {
        var resourceBuilder = ResourceBuilder.CreateDefault();
        ConfigureResource(resourceBuilder);
        return resourceBuilder;
    }

    private static void ConfigureResource(ResourceBuilder resourceBuilder)
    {
        var name = typeof(Program).Assembly.GetName();
        resourceBuilder
            .AddService(
                serviceName: Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "ke-conf-otel-mc-example-service",
                serviceVersion: name.Version!.ToString(),
                autoGenerateServiceInstanceId: true)
            .AddEnvironmentVariableDetector();
    }

    private static void ConfigureOtlp(OtlpExporterOptions otlp)
    {
        otlp.Protocol = OtlpExportProtocol.HttpProtobuf;
        otlp.ExportProcessorType = ExportProcessorType.Simple;
    }
}
