using CsvSupport;
using KustoSupport;
using NotNullStrings;

var context = new KustoQueryContext();

CsvLoader.Load(args.First(), context, "data");
while (true)
{
    Console.Write(">");
    var query = Console.ReadLine().Trim();
    var res = await context.RunTabularQueryAsync(query);
    if (res.Error.IsNotBlank())
        Console.WriteLine(res.Error);
    else
    {
        Console.WriteLine(KustoFormatter.Tabulate(res));
        Console.WriteLine($"{res.QueryDuration}ms");
    }
}