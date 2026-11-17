using Serilog;

namespace tracing_test_service;

public class SomeWorker
{
    public void DoSomeWork()
    {
        Log.Logger.Information("Doing some work");
        //Console.WriteLine("Doing some work");
    }
}