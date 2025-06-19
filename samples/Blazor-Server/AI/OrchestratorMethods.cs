using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using OpenAI;
using Azure.AI.OpenAI;
using KustoLoco.Services;
using System.ClientModel;
using NotNullStrings;

namespace KustoLoco.AI
{
    public class AIResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
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
            var Organization = objSettings.Organization;
            var ApiKey = objSettings.ApiKey;
            var Endpoint = objSettings.Endpoint;
            var ApiVersion = objSettings.ApiVersion;
            var AIEmbeddingModel = objSettings.AIEmbeddingModel;
            var AIModel = objSettings.AIModel;

            var apiKeyCredential = new ApiKeyCredential(ApiKey);

            if (objSettings.AIType == "OpenAI")
            {
                var options = new OpenAIClientOptions
                {
                    NetworkTimeout = TimeSpan.FromSeconds(520)
                };

                return new OpenAIClient(
                    apiKeyCredential, options)
                    .AsChatClient(AIModel);
            }
            else if (objSettings.AIType == "Azure OpenAI") 
            {
                var options = new AzureOpenAIClientOptions
                {
                    NetworkTimeout = TimeSpan.FromSeconds(520)
                };

                return new AzureOpenAIClient(
                    new Uri(Endpoint),
                    apiKeyCredential, options)
                    .AsChatClient(AIModel);
            }
            else // Local LLM
            {
                return new OllamaChatClient(
                    new Uri(Endpoint),
                    AIModel);
            }
        }
        #endregion

        #region public async Task<bool> TestAccess(SettingsService objSettings, string GPTModel)
        public async Task<bool> TestAccess(SettingsService objSettings, string GPTModel)
        {
            var chatClient = CreateAIChatClient(objSettings);
            var SystemMessage = "Please return the following as json: \"This is successful\" in this format {\r\n  'message': message\r\n}";
            var response = await chatClient.GetResponseAsync(SystemMessage);

            return !response.Text.IsBlank();
        }
        #endregion

        #region public string GetTemplate()
        public string GetTemplate()
        {
            // Load template from Templates\AITemplate.txt
            var Template = File.ReadAllText("Templates\\AITemplate.txt");
            return Template;
        }
        #endregion

        /// <summary>
        /// Sends a system message to the AI chat client and returns the JSON response.
        /// </summary>
        /// <param name="objSettings">The settings containing LLM configuration details.</param>
        /// <param name="AIQuery">The query to use to call the LLM</param>
        /// <returns>A JSON string containing the response message, or null if no response is received.</returns>
        #region public async Task<AIResponse> CallOpenAI(SettingsService objSettings, string AIQuery)
        public async Task<AIResponse> CallOpenAI(SettingsService objSettings, string AIQuery)         
        {
            try
            {
                // Create the AI chat client using the provided settings
                var chatClient = CreateAIChatClient(objSettings);

                // Send the system message to the AI chat client
                var response = await chatClient.GetResponseAsync(AIQuery);

                // Check if the response contains any choices
                if (response.Text.IsBlank())
                {
                    // Optionally, log the absence of choices or handle it as needed
                    return new AIResponse() { Code = "", Error = "No choices returned in the AI response." };
                }

                // Extract the text from the first choice
                var jsonResponse = response.Text.Trim();

                jsonResponse = ExtractJsonFromResponse(jsonResponse);

                try
                {
                    var parsedResponse = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);

                    if (parsedResponse != null && !string.IsNullOrEmpty(parsedResponse.Code))
                    {
                        return parsedResponse;
                    }
                    else
                    {
                        return new AIResponse() { Code = "", Error = "Parsed response is null or missing the 'Code' field." };
                    }
                }
                catch (JsonException jsonEx)
                {
                    // Handle JSON parsing errors
                    return new AIResponse() { Code = "", Error = $"Error parsing JSON response: {jsonEx.Message}" };
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                return new AIResponse() { Code = "", Error = $"An error occurred while testing access: {ex.Message}" };
            }
        }
        #endregion

        public static string ExtractJsonFromResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var startIndex = input.IndexOf('{');
            var endIndex = input.LastIndexOf('}');

            // Validate positions
            if (startIndex == -1 || endIndex == -1 || endIndex < startIndex)
                return string.Empty;

            // Extract and return the substring that should represent valid JSON
            return input.Substring(startIndex, endIndex - startIndex + 1);
        }
    }
}
