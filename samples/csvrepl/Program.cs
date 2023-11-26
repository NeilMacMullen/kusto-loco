using CsvSupport;
using Extensions;
using KustoSupport;

var context = new KustoQueryContext();

CsvLoader.Load(args.First(), context, "data");
while (true)
{
    Console.Write(">");
    var query = Console.ReadLine().Trim();
    var res = await context.RunQuery(query);
    if (res.Error.IsNotBlank())
        Console.WriteLine(res.Error);
    else
    {
        Console.WriteLine(KustoFormatter.Tabulate(res.Results));
        Console.WriteLine($"{res.QueryDuration}ms");
    }
}