using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace tracing_test_service;

/// <summary>
/// Единая точка с ActivitySource, Meter и инструментами для кастомной телеметрии.
/// Один ActivitySource на приложение — рекомендованный подход.
/// </summary>
public static class Telemetry
{
    public const string ActivitySourceName = "tracing_test_service";
    public const string MeterName = "tracing_test_service";

    private static readonly string? Version = typeof(Telemetry).Assembly.GetName().Version?.ToString();

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version);

    public static readonly Meter Meter = new(MeterName, Version);

    /// <summary>
    /// Счётчик обработанных запросов. Экспортируется через OTLP/консоль.
    /// </summary>
    public static readonly Counter<long> RequestsCounter = Meter.CreateCounter<long>(
        name: "tracing_test_service.requests",
        unit: "{request}",
        description: "Number of handled requests");
}
