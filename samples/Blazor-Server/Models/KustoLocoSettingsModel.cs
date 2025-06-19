using System.Text.Json.Serialization;

namespace KustoLoco.Models
{
    public class KustoLocoSettingsModel
    {
        [JsonPropertyName("OpenAIServiceOptions")]
        public OpenAIServiceOptions OpenAIServiceOptions { get; set; } = new();

        [JsonPropertyName("ApplicationSettings")]
        public ApplicationSettings ApplicationSettings { get; set; } = new();
    }

    public class OpenAIServiceOptions
    {
        public string Organization { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class ApplicationSettings
    {
        public string AIModel { get; set; } = string.Empty;
        public string AIType { get; set; } = "OpenAI"; // Default value
        public string Endpoint { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string AIEmbeddingModel { get; set; } = string.Empty;
    }
}
