using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    public const string InstrumentationName = "tracing_test_service";

    public static IOpenTelemetryBuilder AddOtelTelemetry(this IOpenTelemetryBuilder openTelemetryBuilder) => openTelemetryBuilder
        .ConfigureResource(resource =>
        {
            resource.AddService(
                serviceName: "название123",
                serviceVersion: typeof(Program).Assembly.GetName().Version!.ToString());
            resource.AddEnvironmentVariableDetector();
        })
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(InstrumentationName)
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter(InstrumentationName)
            .AddConsoleExporter())
        .WithLogging(
            logging => logging.AddConsoleExporter(),
            options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
            })
        .UseOtlpExporter();
}
