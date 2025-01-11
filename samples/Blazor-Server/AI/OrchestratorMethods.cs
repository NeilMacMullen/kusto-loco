using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using OpenAI;
using Azure.AI.OpenAI;
using Azure.Identity;
using KustoLoco.Services;
using System.ClientModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Azure;

namespace KustoLoco.AI
{
    public class AIResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class OrchestratorMethods
    {
        // Constructor
        public OrchestratorMethods()
        {

        }

        // Methods

        #region public IChatClient CreateAIChatClient(SettingsService objSettings)
        public IChatClient CreateAIChatClient(SettingsService objSettings)
        {
            string Organization = objSettings.Organization;
            string ApiKey = objSettings.ApiKey;
            string Endpoint = objSettings.Endpoint;
            string ApiVersion = objSettings.ApiVersion;
            string AIEmbeddingModel = objSettings.AIEmbeddingModel;
            string AIModel = objSettings.AIModel;

            OpenAIClientOptions options = new OpenAIClientOptions();
            options.NetworkTimeout = TimeSpan.FromSeconds(520);

            ApiKeyCredential apiKeyCredential = new ApiKeyCredential(ApiKey);

            if (objSettings.AIType == "OpenAI")
            {
                return new OpenAIClient(
                    apiKeyCredential)
                    .AsChatClient(AIModel);
            }
            else // Azure OpenAI
            {
                return new AzureOpenAIClient(
                    new Uri(Endpoint),
                    apiKeyCredential)
                    .AsChatClient(AIModel);
            }
        }
        #endregion

        #region public async Task<bool> TestAccess(SettingsService objSettings, string GPTModel)
        public async Task<bool> TestAccess(SettingsService objSettings, string GPTModel)
        {
            var chatClient = CreateAIChatClient(objSettings);
            string SystemMessage = "Please return the following as json: \"This is successful\" in this format {\r\n  'message': message\r\n}";
            var response = await chatClient.CompleteAsync(SystemMessage);

            if (response.Choices.Count == 0)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region public string GetTemplate()
        public string GetTemplate()
        {
            // Load template from Templates\AITemplate.txt
            string Template = File.ReadAllText("Templates\\AITemplate.txt");
            return Template;
        }
        #endregion

        /// <summary>
        /// Sends a system message to the AI chat client and returns the JSON response.
        /// </summary>
        /// <param name="objSettings">The settings containing LLM configuration details.</param>
        /// <param name="AIQuery">The query to use to call the LLM</param>
        /// <returns>A JSON string containing the response message, or null if no response is received.</returns>
        #region public async Task<string> CallOpenAI(SettingsService objSettings, string AIQuery)
        public async Task<string> CallOpenAI(SettingsService objSettings, string AIQuery)         
        {
            try
            {
                // Create the AI chat client using the provided settings
                var chatClient = CreateAIChatClient(objSettings);

                // Send the system message to the AI chat client
                var response = await chatClient.CompleteAsync(AIQuery);

                // Check if the response contains any choices
                if (response.Choices == null || response.Choices.Count == 0)
                {
                    // Optionally, log the absence of choices or handle it as needed
                    Console.Error.WriteLine("No choices returned in the AI response.");
                    return null;
                }

                // Extract the text from the first choice
                string jsonResponse = response.Choices[0].Text.Trim();

                // Optionally, parse the JSON to ensure it's in the expected format
                // This step is optional and depends on whether you need to work with the parsed data
                try
                {
                    var parsedResponse = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);
                    if (parsedResponse != null && !string.IsNullOrEmpty(parsedResponse.Message))
                    {
                        return JsonConvert.SerializeObject(parsedResponse, Formatting.Indented);
                    }
                    else
                    {
                        Console.Error.WriteLine("Parsed response is null or missing the 'message' field.");
                        return null;
                    }
                }
                catch (JsonException jsonEx)
                {
                    // Handle JSON parsing errors
                    Console.Error.WriteLine($"Error parsing JSON response: {jsonEx.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                Console.Error.WriteLine($"An error occurred while testing access: {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
