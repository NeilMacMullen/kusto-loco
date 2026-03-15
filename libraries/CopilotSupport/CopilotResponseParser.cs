using System.Text.Json;

namespace KustoLoco.CopilotSupport;

/// <summary>
/// Parses model responses and handles the interchange format
/// </summary>
public static class CopilotResponseParser
{
    /// <summary>
    /// Attempts to parse a model response into structured actions
    /// </summary>
    public static CopilotParseResult Parse(string modelResponse)
    {
        if (string.IsNullOrWhiteSpace(modelResponse))
            return CopilotParseResult.Error("Empty response from model");

        // Remove markdown code blocks if present
        var jsonResponse = modelResponse
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        try
        {
            var response = JsonSerializer.Deserialize<CopilotResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response?.Actions == null || response.Actions.Count == 0)
                return CopilotParseResult.Error("No actions found in response");

            return CopilotParseResult.Success(response);
        }
        catch (JsonException)
        {
            // If we can't parse the structured response, treat the whole thing as an explanation
            return CopilotParseResult.Success(new CopilotResponse
            {
                Actions = [new CopilotAction { Type = "explanation", Text = modelResponse }]
            });
        }
    }
}

/// <summary>
/// Result of parsing a model response
/// </summary>
public class CopilotParseResult
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public CopilotResponse Response { get; private set; } = new();

    public static CopilotParseResult Success(CopilotResponse response) =>
        new() { IsSuccess = true, Response = response };

    public static CopilotParseResult Error(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
