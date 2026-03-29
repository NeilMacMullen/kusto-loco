using System.Text.Json.Serialization;

namespace KustoLoco.CopilotSupport;

/// <summary>
/// Represents a single action returned by the Copilot model
/// </summary>
public class CopilotAction
{
    /// <summary>
    /// The type of action: "explanation", "kql", "command", or "inspect"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// For "explanation" type: the text to display
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// For "kql" and "inspect" types: the KQL query to execute
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// For "command" type: the dot command to execute
    /// </summary>
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;
}

/// <summary>
/// Represents a complete response from the Copilot model
/// </summary>
public class CopilotResponse
{
    /// <summary>
    /// List of actions to perform in sequence
    /// </summary>
    [JsonPropertyName("actions")]
    public List<CopilotAction> Actions { get; set; } = [];
}
