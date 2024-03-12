using System.Collections.Immutable;
using KustoLoco.Core.Evaluation.BuiltIns;
using Kusto.Language.Symbols;
using KustoLoco.Core;


namespace CustomFunctions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var context = new KustoQueryContext();

        //register new function
        var d = new Dictionary<FunctionSymbol, ScalarFunctionInfo>();
        FizzFunction.Register(d);
        context.AddFunctions(d);

        //set up some data
        var numbers = Enumerable.Range(0, 100)
            .Select(i => new Number(i, string.Empty))
            .ToImmutableArray();

        context.AddTableFromRecords("numbers", numbers);
        //run a query using our custom function
        var o = await context.RunTabularQueryAsync(
            "numbers | extend FizzBuzz = fizz(N)");

        foreach (var fb in o.DeserialiseTo<Number>())
            Console.WriteLine(fb);
    }

    private readonly record struct Number(int N, string FizzBuzz);
}