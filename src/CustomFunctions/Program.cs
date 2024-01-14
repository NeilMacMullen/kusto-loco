using BabyKusto.Core.Evaluation.BuiltIns;
using Kusto.Language.Symbols;
using KustoSupport;

namespace CustomFunctions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var d = new Dictionary<FunctionSymbol, ScalarFunctionInfo>();
        FizzFunction.Register(d);

        var numbers = Enumerable.Range(0, 100)
            .Select(i => new Number(i, string.Empty))
            .ToArray();


        var context = new KustoQueryContext();
        context.AddFunctions(d);
        context.AddTableFromRecords("numbers", numbers);
        var o = await context.RunTabularQueryAsync(
            "numbers | extend FizzBuzz = fizz(N)");
        Console.WriteLine(o.Error);
        var x = o.DeserialiseTo<Number>();
        foreach (var fb in x)
            Console.WriteLine(fb);
    }

    private readonly record struct Number(int N, string FizzBuzz);
}