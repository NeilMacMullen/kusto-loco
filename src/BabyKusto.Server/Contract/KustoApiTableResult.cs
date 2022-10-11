using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BabyKusto.Server.Contract
{
    public class KustoApiTableResult
    {
        [JsonPropertyName("TableName")]
        public string? TableName { get; set; }

        [JsonPropertyName("Columns")]
        public List<KustoApiColumnDescription> Columns { get; } = new();

        [JsonPropertyName("Rows")]
        public List<JsonArray> Rows { get; } = new();
    }
}