using KustoLoco.Core;
using LogSetup;
using NLog;
using NotNullStrings;

namespace BasicTests;

public abstract class TestMethods
{
    public TestMethods()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    protected static KustoQueryContext CreateContext() => KustoQueryContext.CreateWithDebug();

    protected async Task<string> LastLineOfResult(string query)
    {
        var context = CreateContext();
        var result = await context.RunQuery(query);
        //return the last line 
        Console.WriteLine($"{result.Error}");
        Console.WriteLine($"{KustoFormatter.Tabulate(result, 10)}");
        return result.GetRow(result.RowCount - 1)
            .Select(KustoFormatter.ObjectToKustoString)
            .JoinString();
    }
}