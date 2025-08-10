using KustoLoco.Core;
using LogSetup;
using NLog;
using NotNullStrings;

namespace BasicTests;

public abstract class TestMethods
{
    protected TestMethods()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    protected static KustoQueryContext CreateContext()
        => KustoQueryContext.CreateForTest();


    protected async Task<KustoQueryResult> Result(string query)
    {
        var context = CreateContext();
        var result = await context.RunQuery(query);
        //return the last line 
        Console.WriteLine($"{result.Error}");
        Console.WriteLine($"{KustoFormatter.Tabulate(result, 10)}");
        return result;
    }
    protected async Task<string> LastLineOfResult(string query)
    {
        var result = await Result(query);
        return result.GetRow(result.RowCount - 1)
            .Select(KustoFormatter.ObjectToKustoString)
            .JoinString();
    }

    protected  Task<string> ResultAsLines(string query)
        => ResultAsString(query, Environment.NewLine);
    protected async Task<string> ResultAsString(string query,string lineSeparator=",")
    {
        var result = await Result(query);
        return result.EnumerateRows().Select(RowString).JoinString(lineSeparator);
    }

    public string RowString(object?[] args)
        => args.Select(KustoFormatter.ObjectToKustoString).JoinString(",");
}
