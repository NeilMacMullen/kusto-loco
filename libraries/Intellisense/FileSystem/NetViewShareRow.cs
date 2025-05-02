using System.Collections;
using System.Text;

namespace Intellisense.FileSystem;

internal record struct NetViewIndexes : IEnumerable<int>
{
    public int ShareName { get; set; }
    public int Type { get; set; }
    public int UsedAs { get; set; }
    public int Comment { get; set; }

    public IEnumerator<int> GetEnumerator() => new List<int> { ShareName, Type, UsedAs, Comment }.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class NetViewShareRow
{
    private const string RowSeparator =
        "-------------------------------------------------------------------------------";

    private const string EndingText = "The command completed successfully.";

    public string ShareName = "";
    public string Type = "";
    public string UsedAs = "";
    public string Comment = "";

    public static IEnumerable<NetViewShareRow> Parse(string text)
    {
        var txt = text
            .ReplaceLineEndings("\n")
            .Split("\n", StringSplitOptions.RemoveEmptyEntries);


        var cols = txt
            .Index()
            .SkipWhile(x => !x.Item.StartsWith("Share name"))
            .Take(1)
            .ToList();

        if (cols is not [var (index, line)])
        {
            return [];
        }

        var colIndexes = new List<int>
        {
            0,
            line.IndexOf("Type", StringComparison.InvariantCultureIgnoreCase),
            line.IndexOf("Used as", StringComparison.InvariantCultureIgnoreCase),
            line.IndexOf("Comment", StringComparison.InvariantCultureIgnoreCase)
        };


        var rows = txt
            .Skip(index + 1)
            .SkipWhile(x => x is not RowSeparator)
            .Skip(1)
            .TakeWhile(x => x is not EndingText)
            .Select(ProcessRow);

        return rows;

        NetViewShareRow ProcessRow(string row)
        {
            var current = -1;
            var sbs = colIndexes.Select(_ => new StringBuilder()).ToList();


            for (var i = 0; i < row.Length; i++)
            {
                if (colIndexes.Contains(i))
                {
                    current++;
                }
                sbs[current].Append(row[i]);
            }

            var r = new NetViewShareRow();
            foreach (var (i,x) in sbs.Select(x => x.ToString().Trim()).Index())
            {
                if (i is 0)
                {
                    r.ShareName = x;
                }
                if (i is 1)
                {
                    r.Type = x;
                }
                if (i is 2)
                {
                    r.UsedAs = x;
                }

                if (i is 3)
                {
                    r.Comment = x;
                }
            }

            return r;
        }
    }
}
