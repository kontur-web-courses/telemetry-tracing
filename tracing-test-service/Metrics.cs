using System.Diagnostics.Metrics;

namespace tracing_test_service;

public class Metrics
{
    private readonly Counter<int> indexRequestsCount;
    private readonly Histogram<double> indexRequestsTime;

    public Metrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(nameof(Metrics));
        indexRequestsCount = meter.CreateCounter<int>("requests.index.count", "pcs", "Количество запросов");
        indexRequestsTime = meter.CreateHistogram<double>("requests.index.time", "ms", "Время запроса к index");
    }

    public void RequestToIndex(TimeSpan elapsed)
    {
        indexRequestsCount.Add(1);
        indexRequestsTime.Record(elapsed.TotalMilliseconds);
    }
}