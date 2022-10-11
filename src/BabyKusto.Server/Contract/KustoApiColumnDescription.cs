using System;
using System.Text.Json.Serialization;
using Kusto.Language.Symbols;

namespace BabyKusto.Server.Contract
{
    public class KustoApiColumnDescription
    {
        [JsonPropertyName("ColumnName")]
        public string? ColumnName { get; set; }

        [JsonPropertyName("DataType")]
        public string? DataType { get; set; }

        [JsonPropertyName("ColumnType")]
        public string? ColumnType { get; set; }

        public static KustoApiColumnDescription Create(string name, TypeSymbol type)
        {
            string dataType;
            if (type == ScalarTypes.Int)
            {
                dataType = "Int32";
            }
            else if (type == ScalarTypes.Long)
            {
                dataType = "Int64";
            }
            else if (type == ScalarTypes.Real)
            {
                dataType = "Double";
            }
            else if (type == ScalarTypes.Bool)
            {
                dataType = "Boolean";
            }
            else if (type == ScalarTypes.String)
            {
                dataType = "String";
            }
            else if (type == ScalarTypes.Guid)
            {
                dataType = "Guid";
            }
            else if (type == ScalarTypes.DateTime)
            {
                dataType = "DateTime";
            }
            else if (type == ScalarTypes.TimeSpan)
            {
                dataType = "TimeSpan";
            }
            else
            {
                throw new ArgumentException($"Type {type.Display} is not supported.", nameof(type));
            }

            return new KustoApiColumnDescription
            {
                ColumnName = name,
                DataType = dataType,
                ColumnType = type.Display,
            };
        }
    }
}