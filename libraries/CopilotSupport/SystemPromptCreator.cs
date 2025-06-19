using System.Reflection;
using System.Text;
using KustoLoco.Core;

namespace KustoLoco.CopilotSupport;

/// <summary>
/// Manages creation of CoPilot system prompt
/// </summary>
/// <remarks>
/// The general prompt is loaded from an embedded resource then
/// schema information about available tables is provided
/// </remarks>
public static class SystemPromptCreator
{
    private static Stream SafeGetResourceStream(string substring)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var availableResources = assembly.GetManifestResourceNames();
        var wanted =
            availableResources.Single(name => name.Contains(substring, StringComparison.CurrentCultureIgnoreCase));
        return assembly.GetManifestResourceStream(wanted)!;
    }

    public static string GetTemplate()
    {
        using var strm = SafeGetResourceStream("AITemplate.txt");
        using var t = new StreamReader(strm);
        return t.ReadToEnd();
    }
   
    public static string CreateSystemPrompt(KustoQueryContext kustoContext)
    {
        // Load template from Templates\AITemplate.txt
        var AIQuery = GetTemplate();

        // Get Table Schema
        string schema = "";
        var sb = new StringBuilder();

        foreach (var table in kustoContext.Tables())
        {
            sb.AppendLine($"The table named '{table.Name}' has the following columns");

            var cols = table.ColumnNames.Zip(table.Type.Columns)
                .Select(z => $"  {z.First} is of type {z.Second.Type.Name}")
                .ToArray();

            foreach (var column in cols)
            {
                sb.AppendLine(column);
            }

            schema = sb.ToString();
        }

        // Replace ## CURRENT SCHEMA ##, ## CURRENT CODE ##, ## CURRENT REQUEST ##
        // with the current schema, code and request
        AIQuery = AIQuery.Replace("## CURRENT SCHEMA ##", schema);
        return AIQuery;
    }
}
