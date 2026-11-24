namespace tracing_test_service;

/// <summary>
/// Настройки OpenTelemetry из секции "OpenTelemetry" (appsettings.json).
/// </summary>
public sealed class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    public string ServiceName { get; set; } = "ke-conf-otel-mc-example-service";

    /// <summary>Печатать ли спаны/метрики/логи в консоль приложения.</summary>
    public bool ConsoleExporter { get; set; } = true;

    /// <summary>Локальный сборщик Jaeger (docker run ... jaegertracing/jaeger).</summary>
    public ExporterOptions Jaeger { get; set; } = new()
    {
        Enabled = true,
        BaseUrl = "http://localhost:4318",
        Signals = ["traces"]
    };

    /// <summary>Общий сервис («труба»), куда сдаются трассировки/метрики/логи.</summary>
    public ExporterOptions Pipe { get; set; } = new()
    {
        Enabled = false,
        BaseUrl = "https://fiit-backend-telemetry.duckdns.org",
        Signals = ["traces", "metrics", "logs"]
    };

    /// <summary>Список включённых OTLP-экспортёров для указанного вида телеметрии.</summary>
    public IEnumerable<(string Name, ExporterOptions Options)> GetEnabledExporters(string signal)
    {
        if (Jaeger.Enabled && Jaeger.Handles(signal))
        {
            yield return ("jaeger", Jaeger);
        }

        if (Pipe.Enabled && Pipe.Handles(signal))
        {
            yield return ("pipe", Pipe);
        }
    }

    public sealed class ExporterOptions
    {
        public bool Enabled { get; set; }

        /// <summary>
        /// Базовый адрес OTLP-приёмника. Путь сигнала (/v1/traces, /v1/metrics, /v1/logs)
        /// добавляется автоматически для HTTP-протокола.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>HttpProtobuf или Grpc.</summary>
        public string Protocol { get; set; } = "HttpProtobuf";

        /// <summary>Какие виды телеметрии отправлять: traces, metrics, logs.</summary>
        public string[] Signals { get; set; } = ["traces", "metrics", "logs"];

        /// <summary>Дополнительные заголовки, например { "Authorization": "Bearer ..." }.</summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        public bool Handles(string signal) =>
            Signals.Any(value => string.Equals(value, signal, StringComparison.OrdinalIgnoreCase));
    }
}
