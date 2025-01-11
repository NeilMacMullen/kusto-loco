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
    public partial class OrchestratorMethods
    {
        // Constructor
        public OrchestratorMethods()
        {

        }

        // Utility Methods

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
    }
}
