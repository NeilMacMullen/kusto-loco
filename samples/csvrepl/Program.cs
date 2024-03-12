
using KustoLoco.FileFormats;
using KustoLoco.Core;
using NotNullStrings;

var result = await new CsvLoader().LoadTable(args.First(), "data", new ConsoleProgressReporter());
if (result.Error.IsNotBlank())
{
    Console.WriteLine(result.Error);
    return;
}

var context = new KustoQueryContext();
context.AddTable(result.Table);

while (true)
{
    Console.Write(">");
    var query = Console.ReadLine().Trim();
    var res = await context.RunTabularQueryAsync(query);
    if (res.Error.IsNotBlank())
    {
        Console.WriteLine(res.Error);
    }
    else
    {
        Console.WriteLine(KustoFormatter.Tabulate(res));
        Console.WriteLine($"{res.QueryDuration}ms");
    }
}