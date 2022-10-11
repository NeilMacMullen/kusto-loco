// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using BabyKusto.Core;

namespace BabyKusto.Server.Service
{
    internal static class JsonSchemaHelper
    {
        public static string GetJsonSchema(BabyKustoServerOptions options, IEnumerable<ITableSource> tables)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _ = tables ?? throw new ArgumentNullException(nameof(tables));

            var database = new DatabaseSchema
            {
                Name = options.DatabaseName,
                MajorVersion = 1,
                MinorVersion = 0,
                DatabaseAccessMode = "ReadWrite",
            };

            foreach (var table in tables)
            {
                var tableSchema = new TableSchema
                {
                    Name = table.Type.Name,
                };
                foreach (var column in table.Type.Columns)
                {
                    tableSchema.OrderedColumns.Add(
                        new ColumnSchema
                        {
                            Name = column.Name,
                            CslType = column.Type.Display,
                            Type = column.Type switch
                            {
                                _ => "",
                            },
                        });
                }
                database.Tables.Add(tableSchema.Name, tableSchema);
            }

            var root = new JsonSchemaResult
            {
                Databases = { { options.DatabaseName, database } },
            };

            return JsonSerializer.Serialize(root);
        }

        private class JsonSchemaResult
        {
            [JsonPropertyName("Databases")]
            public Dictionary<string, DatabaseSchema> Databases { get; } = new();
        }

        private class DatabaseSchema
        {
            [JsonPropertyName("Name")]
            public string? Name { get; set; }

            [JsonPropertyName("MajorVersion")]
            public int? MajorVersion { get; set; }

            [JsonPropertyName("MinorVersion")]
            public int? MinorVersion { get; set; }

            [JsonPropertyName("DatabaseAccessMode")]
            public string? DatabaseAccessMode { get; set; }

            [JsonPropertyName("Tables")]
            public Dictionary<string, TableSchema> Tables { get; } = new();

            [JsonPropertyName("ExternalTables")]
            public Dictionary<string, object> ExternalTables { get; } = new();

            [JsonPropertyName("MaterializedViews")]
            public Dictionary<string, object> MaterializedViews { get; } = new();

            [JsonPropertyName("EntityGroups")]
            public Dictionary<string, object> EntityGroups { get; } = new();

            [JsonPropertyName("Functions")]
            public Dictionary<string, object> Functions { get; } = new();
        }

        private class TableSchema
        {
            [JsonPropertyName("Name")]
            public string? Name { get; set; }

            [JsonPropertyName("Folder")]
            public string? Folder { get; set; }

            [JsonPropertyName("DocString")]
            public string? DocString { get; set; }

            [JsonPropertyName("OrderedColumns")]
            public List<ColumnSchema> OrderedColumns { get; } = new();
        }

        private class ColumnSchema
        {
            [JsonPropertyName("Name")]
            public string? Name { get; set; }

            [JsonPropertyName("Type")]
            public string? Type { get; set; }

            [JsonPropertyName("CslType")]
            public string? CslType { get; set; }
        }
    }
}
