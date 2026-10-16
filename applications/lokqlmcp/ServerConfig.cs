using System.Text;

namespace LokqlMcp;

/// <summary>
/// Optional configuration read from a simple markdown file:
/// "# Context" holds overall text, "# Tables" holds "## tableName" subsections.
/// </summary>
public sealed class ServerConfig
{
    public static readonly ServerConfig Empty = new(null,
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

    public ServerConfig(string? context, IReadOnlyDictionary<string, string> tableDescriptions)
    {
        Context = string.IsNullOrWhiteSpace(context) ? null : context;
        TableDescriptions = tableDescriptions;
    }

    public string? Context { get; }
    public IReadOnlyDictionary<string, string> TableDescriptions { get; }

    public static ServerConfig Load(string path) => Parse(File.ReadAllText(path));

    /// <summary>
    /// Returns config text guaranteed to contain a "## name" entry under "# Tables" for each name.
    /// Existing text is preserved; missing entries are added at the end of the Tables section.
    /// </summary>
    public static string AddMissingTables(string? text, IEnumerable<string> tableNames, out IReadOnlyList<string> added)
    {
        text ??= "";
        var existing = Parse(text).TableDescriptions;
        var missing = tableNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(n => !existing.ContainsKey(n))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
        added = missing;

        var lines = text.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        if (lines.Count == 1 && lines[0].Length == 0)
            lines.Clear();
        if (missing.Count == 0)
            return text;

        var inFence = false;
        var tablesStart = -1;
        var tablesEnd = lines.Count;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].TrimStart().StartsWith("```"))
                inFence = !inFence;
            if (inFence || !lines[i].StartsWith("# "))
                continue;
            if (tablesStart >= 0)
            {
                tablesEnd = i;
                break;
            }

            if (lines[i][2..].Trim().Equals("tables", StringComparison.OrdinalIgnoreCase))
                tablesStart = i;
        }

        if (tablesStart < 0)
        {
            if (lines.Count == 0)
            {
                lines.Add("# Context");
                lines.Add("");
            }

            lines.Add("# Tables");
            tablesStart = lines.Count - 1;
            tablesEnd = lines.Count;
        }

        var insertAt = tablesEnd;
        while (insertAt > tablesStart + 1 && string.IsNullOrWhiteSpace(lines[insertAt - 1]))
            insertAt--;

        var block = new List<string>();
        foreach (var name in missing)
        {
            block.Add("");
            block.Add($"## {name}");
            block.Add("");
        }

        if (insertAt < lines.Count)
            block.Add("");
        lines.RemoveRange(insertAt, tablesEnd - insertAt);
        lines.InsertRange(insertAt, block);
        return string.Join(Environment.NewLine, lines).TrimEnd() + Environment.NewLine;
    }

    public static ServerConfig Parse(string text)
    {
        var context = new StringBuilder();
        var tables = new Dictionary<string, StringBuilder>(StringComparer.OrdinalIgnoreCase);
        var section = "";
        StringBuilder? current = null;
        var inFence = false;

        foreach (var raw in text.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            if (line.TrimStart().StartsWith("```"))
                inFence = !inFence;

            if (!inFence && line.StartsWith("## ") && section == "tables")
            {
                var name = line[3..].Trim();
                current = new StringBuilder();
                tables[name] = current;
                continue;
            }

            if (!inFence && line.StartsWith("# ") )
            {
                section = line[2..].Trim().ToLowerInvariant();
                current = section == "context" ? context : null;
                continue;
            }

            current?.AppendLine(line);
        }

        return new ServerConfig(context.ToString().Trim(),
            tables.ToDictionary(kv => kv.Key, kv => kv.Value.ToString().Trim(),
                StringComparer.OrdinalIgnoreCase));
    }
}
