using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace tracing_test_service;

public static class OpenTelemetryExtensions
{
    private const string TracesPath = "/v1/traces";
    private const string MetricsPath = "/v1/metrics";
    private const string LogsPath = "/v1/logs";

    /// <summary>
    /// Подключает все три вида телеметрии: трассировки, метрики и логи.
    /// Экспортёры (Jaeger / «труба») настраиваются в секции "OpenTelemetry" appsettings.json.
    /// </summary>
    public static OpenTelemetryBuilder AddOtel(this OpenTelemetryBuilder builder, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(OpenTelemetryOptions.SectionName)
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: options.ServiceName,
                serviceVersion: typeof(OpenTelemetryExtensions).Assembly.GetName().Version?.ToString(),
                autoGenerateServiceInstanceId: true)
            .AddEnvironmentVariableDetector();

        builder
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Регистрируем наши ActivitySource, иначе StartActivity вернёт null.
                    .AddSource(Telemetry.ActivitySourceName);

                if (options.ConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }

                foreach (var (name, exporter) in options.GetEnabledExporters("traces"))
                {
                    tracing.AddOtlpExporter(name, otlp => ConfigureOtlp(otlp, exporter, TracesPath));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Регистрируем наши Meter'ы с кастомными метриками.
                    .AddMeter(Telemetry.MeterName);

                if (options.ConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }

                foreach (var (name, exporter) in options.GetEnabledExporters("metrics"))
                {
                    metrics.AddOtlpExporter(name, otlp => ConfigureOtlp(otlp, exporter, MetricsPath));
                }
            })
            .WithLogging(
                _ => { },
                logging =>
                {
                    logging
                        .SetResourceBuilder(resourceBuilder)
                        .IncludeFormattedMessage = true;
                    logging.IncludeScopes = true;

                    if (options.ConsoleExporter)
                    {
                        logging.AddConsoleExporter();
                    }

                    foreach (var (name, exporter) in options.GetEnabledExporters("logs"))
                    {
                        logging.AddOtlpExporter(name, otlp => ConfigureOtlp(otlp, exporter, LogsPath));
                    }
                });

        return builder;
    }

    private static void ConfigureOtlp(OtlpExporterOptions otlp, OpenTelemetryOptions.ExporterOptions exporter, string signalPath)
    {
        otlp.Endpoint = BuildEndpoint(exporter.BaseUrl, signalPath, exporter.Protocol);
        otlp.Protocol = ParseProtocol(exporter.Protocol);
        otlp.ExportProcessorType = ExportProcessorType.Simple;

        if (exporter.Headers.Count > 0)
        {
            otlp.Headers = string.Join(",", exporter.Headers.Select(header => $"{header.Key}={header.Value}"));
        }
    }

    private static Uri BuildEndpoint(string baseUrl, string signalPath, string protocol)
    {
        var trimmed = baseUrl.TrimEnd('/');

        // Для gRPC путь сигнала не используется, а для HTTP его нужно указать явно,
        // так как при установке Endpoint SDK не добавляет его автоматически.
        if (!trimmed.EndsWith(signalPath, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(protocol, "Grpc", StringComparison.OrdinalIgnoreCase))
        {
            trimmed += signalPath;
        }

        return new Uri(trimmed);
    }

    private static OtlpExportProtocol ParseProtocol(string protocol) =>
        Enum.TryParse<OtlpExportProtocol>(protocol, ignoreCase: true, out var parsed)
            ? parsed
            : OtlpExportProtocol.HttpProtobuf;
}
