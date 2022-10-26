using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BabyKusto.Server.Contract
{
    public class KustoApiResult
    {
        [JsonPropertyName("Tables")]
        public List<KustoApiTableResult> Tables { get; } = new();

        public override string ToString()
        {
            return $"{Tables.Count} table{(Tables.Count == 1 ? "" : "s")}: {string.Join(", ", Tables.Select(t => t.TableName))}";
        }
    }
}