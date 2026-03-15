using System.Reflection;
using System.Text;
using KustoLoco.Core;

namespace KustoLoco.CopilotSupport;

/// <summary>
/// Creates system prompts for the modernized Copilot feature
/// </summary>
public static class CopilotPromptBuilder
{
    private static Stream SafeGetResourceStream(string substring)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var availableResources = assembly.GetManifestResourceNames();
        var wanted = availableResources.Single(name => 
            name.Contains(substring, StringComparison.CurrentCultureIgnoreCase));
        return assembly.GetManifestResourceStream(wanted)!;
    }

    public static string GetCopilotTemplate()
    {
        using var stream = SafeGetResourceStream("CopilotSystemPrompt.txt");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Creates a system prompt with current schema information
    /// </summary>
    public static string CreateSystemPrompt(
        Func<string[]> getAvailableCommands,
        Func<(string TableName, string ColumnName, string ColumnType)[]> getSchemaInfo)
    {
        var template = GetCopilotTemplate();

        // Build commands section
        var commandsBuilder = new StringBuilder();
        var commands = getAvailableCommands();
        foreach (var command in commands)
        {
            commandsBuilder.AppendLine($"- {command}");
        }

        // Build schema section
        var schemaBuilder = new StringBuilder();
        var schemaInfo = getSchemaInfo();
        var tableGroups = schemaInfo.GroupBy(s => s.TableName);

        foreach (var table in tableGroups)
        {
            schemaBuilder.AppendLine($"Table '{table.Key}':");
            foreach (var col in table)
            {
                schemaBuilder.AppendLine($"  - {col.ColumnName}: {col.ColumnType}");
            }
            schemaBuilder.AppendLine();
        }

        if (schemaInfo.Length == 0)
        {
            schemaBuilder.AppendLine("No tables are currently loaded. The user may load data using .load commands or by dragging files into the application.");
        }

        template = template.Replace("## AVAILABLE_COMMANDS ##", commandsBuilder.ToString());
        template = template.Replace("## CURRENT_SCHEMA ##", schemaBuilder.ToString());

        return template;
    }

    /// <summary>
    /// Creates a follow-up message containing inspect results
    /// </summary>
    public static string CreateInspectResultMessage(string query, string result, bool isError)
    {
        if (isError)
        {
            return $"""
                The inspect query failed:
                Query: {query}
                Error: {result}

                Please adjust your approach and try again.
                """;
        }

        return $"""
            Results from your inspect query:
            Query: {query}
            Results:
            {result}

            Continue with your analysis based on this information.
            """;
    }

    /// <summary>
    /// Creates a message to inform the model about a query error
    /// </summary>
    public static string CreateQueryErrorMessage(string query, string error)
    {
        return $"""
            The KQL query failed to execute:
            Query: {query}
            Error: {error}

            Please analyze the error and provide a corrected query. Remember the KQL engine limitations mentioned in your instructions.
            """;
    }
}
