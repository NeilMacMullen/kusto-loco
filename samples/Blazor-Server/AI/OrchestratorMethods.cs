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
    }
}
