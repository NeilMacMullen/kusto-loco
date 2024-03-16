using System.Collections.Immutable;
using Kusto.Language.Symbols;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation.BuiltIns;

namespace CustomFunctions;

internal class Program
{
    private static async Task Main()
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

        context.AddTableFromImmutableData("numbers", numbers);

        //run a query using our custom function
        var query = "numbers | extend FizzBuzz = fizz(N)";
        var results = await context.RunTabularQueryAsync(query);

        foreach (var fb in results.ToRecords<Number>())
            Console.WriteLine(fb);
    }

    private readonly record struct Number(int N, string FizzBuzz);
}