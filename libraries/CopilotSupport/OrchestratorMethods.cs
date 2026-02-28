using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using NotNullStrings;
using OpenAI;

namespace KustoLoco.CopilotSupport;

public class AISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string AIModel { get; set; } = string.Empty;
    public string AIType { get; set; } = "OpenAI"; // Default value
    public string Endpoint { get; set; } = string.Empty;
}

public readonly record struct ModelResponse(string Error, string Response);

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

        if (objSettings.AIType == "OpenAI")
            return new OpenAIClient(
                    apiKeyCredential)
                .GetChatClient(AIModel)
                .AsIChatClient();

        // Azure OpenAI
        return new AzureOpenAIClient(
                new Uri(Endpoint),
                apiKeyCredential)
            .GetChatClient(AIModel)
            .AsIChatClient();
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

            var returnedText= response.Text.NullToEmpty();
            return new ModelResponse{Response =returnedText};
        }
        catch (Exception ex)
        {
            // Handle unexpected exceptions
            return new ModelResponse() { Error = $"Exception:{ex.Message}" };
        }
    }

    #endregion

   
}
