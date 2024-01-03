using Extensions;
using KustoSupport;
using LogSetup;
using NLog;

namespace BasicTests;

public abstract class TestMethods
{
    public TestMethods()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    protected static KustoQueryContext CreateContext() => KustoQueryContext.WithFullDebug();

    protected async Task<string> LastLineOfResult(string query)
    {
        var context = CreateContext();
        var result = await context.RunTabularQueryAsync(query);
        //return the last line 
        Console.WriteLine($"{result.Error}");
        Console.WriteLine($"{KustoFormatter.Tabulate(result, 10)}");
        return result.GetRow(result.Height - 1)
            .Select(KustoFormatter.ObjectToKustoString)
            .JoinString();
    }
}