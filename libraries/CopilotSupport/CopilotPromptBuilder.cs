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

        // Build commands section - only include key commands, not every verb
        var commandsBuilder = new StringBuilder();
        var commands = getAvailableCommands();
        // Limit to most useful commands to reduce prompt size
        var keyCommands = commands.Take(20).ToArray();
        foreach (var command in keyCommands)
        {
            commandsBuilder.AppendLine($"- {command}");
        }
        if (commands.Length > keyCommands.Length)
        {
            commandsBuilder.AppendLine($"- ... and {commands.Length - keyCommands.Length} more commands (use .help for full list)");
        }

        // Build schema section - keep it compact
        var schemaBuilder = new StringBuilder();
        var schemaInfo = getSchemaInfo();
        var tableGroups = schemaInfo.GroupBy(s => s.TableName).ToArray();

        foreach (var table in tableGroups)
        {
            var columns = table.ToArray();
            var columnCount = columns.Length;

            // Show first 8 columns, then summarize the rest
            const int maxColumnsToShow = 8;
            var columnsToShow = columns.Take(maxColumnsToShow)
                .Select(c => $"{c.ColumnName}({c.ColumnType})");

            var columnList = string.Join(", ", columnsToShow);
            if (columnCount > maxColumnsToShow)
            {
                columnList += $", ... +{columnCount - maxColumnsToShow} more";
            }

            schemaBuilder.AppendLine($"Table '{table.Key}' ({columnCount} columns): {columnList}");
        }

        if (schemaInfo.Length == 0)
        {
            schemaBuilder.AppendLine("No tables are currently loaded. Use .load commands to load data.");
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
        var truncatedResult = TruncateIfNeeded(result, 2000);

        if (isError)
        {
            return $"""
                The inspect query failed:
                Query: {query}
                Error: {truncatedResult}

                Please adjust your approach and try again. You MUST respond with a JSON actions block.
                """;
        }

        return $"""
            Inspect results:
            Query: {query}
            Results:
            {truncatedResult}

            IMPORTANT: Continue with your analysis. You MUST respond with a JSON actions block that progresses toward answering the user's question. Do NOT stop here.
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
            Error: {TruncateIfNeeded(error, 500)}

            Please analyze the error and provide a corrected query. Remember the KQL engine limitations mentioned in your instructions.
            """;
    }

    /// <summary>
    /// Creates a follow-up message containing command execution results
    /// </summary>
    public static string CreateCommandResultMessage(string command, string result, bool isError)
    {
        var truncatedResult = TruncateIfNeeded(result, 2000);

        if (isError)
        {
            return $"""
                The command failed:
                Command: {command}
                Error: {truncatedResult}

                Please adjust your approach and try again.
                """;
        }

        return $"""
            Command executed successfully:
            Command: {command}
            Output:
            {truncatedResult}

            Continue with your task.
            """;
    }

    /// <summary>
    /// Truncates a string if it exceeds the maximum length to reduce token usage
    /// </summary>
    private static string TruncateIfNeeded(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text[..maxLength] + $"\n... (truncated, {text.Length - maxLength} more characters)";
    }
}
