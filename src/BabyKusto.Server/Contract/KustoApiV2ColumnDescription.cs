using System.Text.Json.Serialization;

namespace BabyKusto.Server.Contract
{
    public class KustoApiV2ColumnDescription
    {
        [JsonPropertyName("ColumnName")]
        public string? ColumnName { get; set; }

        [JsonPropertyName("ColumnType")]
        public string? ColumnType { get; set; }

        public override string ToString()
        {
            return $"{ColumnName}: {ColumnType}";
        }
    }
}