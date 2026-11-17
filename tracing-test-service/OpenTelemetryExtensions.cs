using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public static OpenTelemetryBuilder AddOtelTracing(this OpenTelemetryBuilder builder)
    {
        var version = typeof(Program).Assembly.GetName().Version?.ToString();
        builder.ConfigureResource(resource => resource
            .AddService("Vashchenko-Novikov-Zueva", serviceVersion: version, autoGenerateServiceInstanceId: true)
            .AddEnvironmentVariableDetector());

        return builder
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(Telemetry.SourceName)
                .AddConsoleExporter()
                .AddOtlpExporter(options => options.Protocol = OtlpExportProtocol.HttpProtobuf))
            .WithMetrics(metrics => metrics
                .AddMeter(Telemetry.SourceName)
                .AddOtlpExporter(options => options.Protocol = OtlpExportProtocol.HttpProtobuf))
            .WithLogging(logging => logging
                .AddOtlpExporter(options => options.Protocol = OtlpExportProtocol.HttpProtobuf));
    }
}
