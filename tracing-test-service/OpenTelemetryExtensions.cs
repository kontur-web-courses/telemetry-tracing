using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public const string SourceName = "tracing_test_service";

    public static OpenTelemetryBuilder AddOtelTracing(this OpenTelemetryBuilder openTelemetryBuilder) => openTelemetryBuilder
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = new Uri("http://localhost:4318/v1/traces");
                    otlp.Protocol = OtlpExportProtocol.HttpProtobuf;
                    otlp.ExportProcessorType = ExportProcessorType.Simple;
                })
                .ConfigureResource(rb =>
                {
                    var name = typeof(Program).Assembly.GetName();
                    rb.AddService(
                        serviceName: "ke-conf-otel-mc-example-service",
                        serviceVersion: name.Version!.ToString(),
                        autoGenerateServiceInstanceId: true);
                    rb.AddEnvironmentVariableDetector();
                })
                .AddSource(SourceName)
                .AddHttpClientInstrumentation();
        });

    public static IOpenTelemetryBuilder AddOtelAll(this IOpenTelemetryBuilder builder) => builder
        .ConfigureResource(rb => rb.AddService(
            serviceName: "WeatherApp10.11",
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