using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
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
                .AddSource("tracing_test_service")
                .AddHttpClientInstrumentation();
        });
}

/**docker run --rm --name jaeger -p 5778:5778 -p 16686:16686 -p 4317:4317 -p 4318:4318 -p 14250:14250 -p 14268:14268 -p 9411:9411 jaegertracing/jaeger:2.3.0 --set receivers.otlp.protocols.http.endpoint=0.0.0.0:4318 --set receivers.otlp.protocols.grpc.endpoint=0.0.0.0:4317*/