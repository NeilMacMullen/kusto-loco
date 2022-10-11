using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BabyKusto.Server.Contract
{
    public class KustoApiMgmtRequestBody
    {
        [JsonPropertyName("csl")]
        public string? Csl { get; set; }

        [JsonPropertyName("db")]
        public string? DB { get; set; }

        [JsonPropertyName("options")]
        public JsonObject? Options { get; set; }
    }
}