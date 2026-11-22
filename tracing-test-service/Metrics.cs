using System.Diagnostics.Metrics;

namespace tracing_test_service;

public class Metrics
{
    public const string MeterName = "tracing_test_service";
    private readonly Counter<int> requestCount;
    private readonly Histogram<double> requestDuration;

    public Metrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        requestCount = meter.CreateCounter<int>("requests.count", "pcs", "Количество запросов");
        requestDuration = meter.CreateHistogram<double>("requests.duration", "ms", "Время запроса");
    }

    public void RecordRequest(TimeSpan elapsed)
    {
        requestCount.Add(1);
        requestDuration.Record(elapsed.TotalMilliseconds);
    }
}
