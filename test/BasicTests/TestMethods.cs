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
        var result = await context.RunQuery(query);
        //return the last line 
        Console.WriteLine($"{result.Error}");
        return KustoFormatter.Tabulate(result.Results)
            .Trim()
            .Split(Environment.NewLine)
            .Last().Trim();
    }

}