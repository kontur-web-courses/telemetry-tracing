using System.Diagnostics.Metrics;

namespace tracing_test_service;

public class Metrics
{
    public const string MeterName = "tracing_test_service";

    private readonly Counter<long> helloRequests;

    public Metrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        helloRequests = meter.CreateCounter<long>(
            "hello.requests.count",
            unit: "requests",
            description: "Number of handled hello requests");
    }

    public void HelloRequestHandled() => helloRequests.Add(1);
}
