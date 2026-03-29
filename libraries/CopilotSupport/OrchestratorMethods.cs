using System.ClientModel;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using NotNullStrings;
using OpenAI;

namespace KustoLoco.CopilotSupport;

public class AISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string AIModel { get; set; } = string.Empty;
    public string AIType { get; set; } = "OpenAI"; // Default value: "OpenAI", "AzureOpenAI", or "Anthropic"
    public string Endpoint { get; set; } = string.Empty;
}

public readonly record struct ModelResponse(string Error, string Response, int InputTokens = 0, int OutputTokens = 0)
{
    public int TotalTokens => InputTokens + OutputTokens;
}

public class OrchestratorMethods
{
    // Constructor

    // Methods

    #region public IChatClient CreateAIChatClient(SettingsService objSettings)

    public static IChatClient CreateAIChatClient(AISettings objSettings)
    {
        var ApiKey = objSettings.ApiKey;
        var Endpoint = objSettings.Endpoint;
        var AIModel = objSettings.AIModel;

        var apiKeyCredential = new ApiKeyCredential(ApiKey);

        switch (objSettings.AIType.ToLowerInvariant())
        {
            case "openai":
                return new OpenAIClient(apiKeyCredential)
                    .GetChatClient(AIModel)
                    .AsIChatClient();

            case "azureopenai":
            case "azure":
                return new AzureOpenAIClient(
                        new Uri(Endpoint),
                        apiKeyCredential)
                    .GetChatClient(AIModel)
                    .AsIChatClient();

            case "anthropic":
            case "claude":
                // Return a wrapper that implements IChatClient for Anthropic
                return new AnthropicChatClient(ApiKey, AIModel);

            default:
                // Default to OpenAI
                return new OpenAIClient(apiKeyCredential)
                    .GetChatClient(AIModel)
                    .AsIChatClient();
        }
    }

    #endregion

    #region public async Task<bool> TestAccess(SettingsService objSettings, string GPTModel)

    public async Task<bool> TestAccess(AISettings objSettings, string GPTModel)
    {
        var chatClient = CreateAIChatClient(objSettings);
        var SystemMessage =
            "Please return the following as json: \"This is successful\" in this format {\r\n  'message': message\r\n}";

        var response = await chatClient.GetResponseAsync(SystemMessage);

        return response.Messages.Count != 0;
    }

    #endregion

    /// <summary>
    ///     Sends a system message to the AI chat client and returns the JSON response.
    /// </summary>
    /// <returns>A JSON string containing the response message, or null if no response is received.</returns>

    #region public async Task<AIResponse> CallOpenAI(SettingsService objSettings, string AIQuery)



    public static async Task<ModelResponse> CallOpenAI(IChatClient chatClient, List<ChatMessage> history)
    {
        try
        {

            // Send the system message to the AI chat client
            var response = await chatClient.GetResponseAsync(history);

            // Check if the response contains any choices
            if (response.Text.IsBlank())
                // Optionally, log the absence of choices or handle it as needed
                return new ModelResponse() { Error = "No choices returned in the AI response." };

            var returnedText = response.Text.NullToEmpty();

            // Extract token usage if available
            var inputTokens = (int)(response.Usage?.InputTokenCount ?? 0);
            var outputTokens = (int)(response.Usage?.OutputTokenCount ?? 0);

            return new ModelResponse(
                Error: string.Empty, 
                Response: returnedText,
                InputTokens: inputTokens,
                OutputTokens: outputTokens);
        }
        catch (Exception ex)
        {
            // Handle unexpected exceptions
            return new ModelResponse() { Error = $"Exception:{ex.Message}" };
        }
    }

    #endregion

   
}
