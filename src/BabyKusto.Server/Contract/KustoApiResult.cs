using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BabyKusto.Server.Contract
{
    public class KustoApiResult
    {
        [JsonPropertyName("Tables")]
        public List<KustoApiTableResult> Tables { get; } = new();
    }
}