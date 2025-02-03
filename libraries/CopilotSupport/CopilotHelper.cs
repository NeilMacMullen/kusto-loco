using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.Core.Settings;

namespace KustoLoco.CopilotSupport;

public class AIResponse
{
    public string Code { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class CopilotHelper
{
    private readonly ChatSession _session;

    public CopilotHelper(KustoSettingsProvider settings, KustoQueryContext kustoContext)
    {
        var asettings = new AISettings
        {
            ApiKey = settings.GetOr("copilot.apikey", string.Empty),
            AIModel = settings.GetOr("copilot.model", ""),
            AIType = settings.GetOr("copilot.type", "OpenAI"),
            Endpoint = settings.GetOr("copilot.endpoint", string.Empty)
        };
        var systemInstructions = SystemPromptCreator.CreateSystemPrompt(kustoContext);
        _session = ChatSession.Create(asettings, systemInstructions);
    }

    public async Task<AIResponse> SendUserRequest(string request)
    {
        var resp = await _session.SendUserRequest(request);
        var jsonResponse = resp.Response;
        // Remove ```json    and ```
        jsonResponse = jsonResponse
            .Replace("```json", "")
            .Replace("```", "");

        try
        {
            var parsedResponse = JsonSerializer.Deserialize<AIResponse>(jsonResponse);

            if (parsedResponse != null && !string.IsNullOrEmpty(parsedResponse.Code))
                return parsedResponse;

            return new AIResponse { Code = "", Error = "Parsed response is null or missing the 'Code' field." };
        }
        catch (JsonException jsonEx)
        {
            // Handle JSON parsing errors
            return new AIResponse { Code = "", Error = $"Error parsing JSON response: {jsonEx.Message}" };
        }
    }
}
