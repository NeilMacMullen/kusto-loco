using System.Text.Json;
using System.Text.RegularExpressions;

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

        // Try to extract JSON from the response
        var jsonResponse = ExtractJson(modelResponse);

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

    /// <summary>
    /// Extracts JSON from model response, handling various formats
    /// </summary>
    private static string ExtractJson(string modelResponse)
    {
        // First, try to find JSON in a markdown code block
        // Pattern: ```json ... ``` or ``` ... ```
        var codeBlockMatch = Regex.Match(modelResponse, @"```(?:json)?\s*(\{[\s\S]*?\})\s*```", RegexOptions.Singleline);
        if (codeBlockMatch.Success)
        {
            return codeBlockMatch.Groups[1].Value.Trim();
        }

        // If no code block, try to find a JSON object directly
        // Look for the first { and last } that form a valid JSON structure
        var firstBrace = modelResponse.IndexOf('{');
        var lastBrace = modelResponse.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            var potentialJson = modelResponse.Substring(firstBrace, lastBrace - firstBrace + 1);

            // Quick validation: check if it looks like our expected format
            if (potentialJson.Contains("\"actions\""))
            {
                return potentialJson.Trim();
            }
        }

        // Fallback: just strip markdown markers and return
        return modelResponse
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();
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
