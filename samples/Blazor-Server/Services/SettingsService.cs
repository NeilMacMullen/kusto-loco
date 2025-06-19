using Blazored.LocalStorage;
using KustoLoco.Models;

namespace KustoLoco.Services
{
    public class SettingsService
    {
        // Properties
        public string Organization { get; private set; } = string.Empty;
        public string ApiKey { get; private set; } = string.Empty;
        public string AIModel { get; private set; } = string.Empty;
        public string AIType { get; private set; } = "OpenAI"; // Default value
        public string Endpoint { get; private set; } = string.Empty;
        public string AIEmbeddingModel { get; private set; } = string.Empty;
        public string ApiVersion { get; private set; } = string.Empty;

        private readonly ILocalStorageService _localStorage;
        private const string SettingsKey = "KustoLocoSettings";

        // Constructor
        public SettingsService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        /// <summary>
        /// Initializes the settings by loading them from local storage.
        /// Call this method after the component has been rendered on the client.
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadSettingsAsync();
        }

        /// <summary>
        /// Loads the settings from local storage.
        /// </summary>
        public async Task LoadSettingsAsync()
        {
            var settings = await _localStorage.GetItemAsync<KustoLocoSettingsModel>(SettingsKey);

            if (settings == null)
            {
                // If settings are not found, initialize with default values
                settings = new KustoLocoSettingsModel();
                await _localStorage.SetItemAsync(SettingsKey, settings);
            }

            // Ensure AIType has a default value if not set
            if (string.IsNullOrWhiteSpace(settings.ApplicationSettings.AIType))
            {
                settings.ApplicationSettings.AIType = "OpenAI";
                await _localStorage.SetItemAsync(SettingsKey, settings);
            }

            // Update properties
            Organization = settings.OpenAIServiceOptions.Organization;
            ApiKey = settings.OpenAIServiceOptions.ApiKey;
            AIModel = settings.ApplicationSettings.AIModel;
            AIType = settings.ApplicationSettings.AIType;
            Endpoint = settings.ApplicationSettings.Endpoint;
            ApiVersion = settings.ApplicationSettings.ApiVersion;
            AIEmbeddingModel = settings.ApplicationSettings.AIEmbeddingModel;
        }

        /// <summary>
        /// Saves the provided settings to local storage.
        /// </summary>
        public async Task SaveSettingsAsync(
            string paramOrganization,
            string paramApiKey,
            string paramAIModel,
            string paramAIType,
            string paramEndpoint,
            string paramApiVersion,
            string paramAIEmbeddingModel)
        {
            var settings = new KustoLocoSettingsModel
            {
                OpenAIServiceOptions = new OpenAIServiceOptions
                {
                    Organization = paramOrganization,
                    ApiKey = paramApiKey
                },
                ApplicationSettings = new ApplicationSettings
                {
                    AIModel = paramAIModel,
                    AIType = string.IsNullOrWhiteSpace(paramAIType) ? "OpenAI" : paramAIType,
                    Endpoint = paramEndpoint,
                    ApiVersion = paramApiVersion,
                    AIEmbeddingModel = paramAIEmbeddingModel
                }
            };

            // Save to local storage
            await _localStorage.SetItemAsync(SettingsKey, settings);

            // Update properties
            Organization = paramOrganization;
            ApiKey = paramApiKey;
            AIModel = paramAIModel;
            AIType = settings.ApplicationSettings.AIType;
            Endpoint = paramEndpoint;
            ApiVersion = paramApiVersion;
            AIEmbeddingModel = paramAIEmbeddingModel;
        }
    }
}
