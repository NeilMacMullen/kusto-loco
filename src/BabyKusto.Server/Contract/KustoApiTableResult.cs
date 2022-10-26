using System.Collections.Generic;
using System.Linq;
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

        public override string ToString()
        {
            return $"{TableName}: ({string.Join(", ", Columns.Select(c => $"{c.ColumnName}: {c.ColumnType}"))}), {Rows.Count} rows";
        }
    }
}