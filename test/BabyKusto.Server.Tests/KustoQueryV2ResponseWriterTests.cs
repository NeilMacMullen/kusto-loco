// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BabyKusto.Core;
using BabyKusto.Server.Contract;
using BabyKusto.Server.Service;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BabyKusto.Server.Tests
{
    public class KustoQueryV2ResponseWriterTests
    {
        [Fact]
        public async Task ZeroTables_Works()
        {
            // Arrange
            var columns = new[] {
                new KustoApiV2ColumnDescription{ ColumnName = "MyColumn", ColumnType = "string" },
            };
            var stream = new MemoryStream();

            // Act
            using (var jsonWriter = new Utf8JsonWriter(stream))
            using (var sut = new KustoQueryV2ResponseWriter(jsonWriter))
            {
                await sut.StartAsync();
                await sut.FinishAsync();
            }

            // Assert
            string result;
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            result.Should().Be("[{'FrameType':'DataSetHeader','IsProgressive':false,'Version':'v2.0'},{'FrameType':'DataSetCompletion','HasErrors':false,'Cancelled':false}]".Replace("\'", "\""));
        }

        [Fact]
        public async Task OneTable_Works()
        {
            // Arrange
            var columns = new[] {
                new KustoApiV2ColumnDescription{ ColumnName = "MyColumn", ColumnType = "string" },
            };
            var stream = new MemoryStream();

            // Act
            using (var jsonWriter = new Utf8JsonWriter(stream))
            using (var sut = new KustoQueryV2ResponseWriter(jsonWriter))
            {
                await sut.StartAsync();

                using (var tableWriter = sut.CreateTableWriter())
                {
                    await tableWriter.StartAsync(0, KustoQueryV2ResponseTableKind.PrimaryResult, "PrimaryResult", columns);
                    tableWriter.StartRow();
                    tableWriter.WriteRowValue(JsonValue.Create("a"));
                    tableWriter.EndRow();
                    await tableWriter.FinishAsync();
                }

                await sut.FinishAsync();
            }

            // Assert
            string result;
            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            result.Should().Be("[{'FrameType':'DataSetHeader','IsProgressive':false,'Version':'v2.0'},{'FrameType':'DataTable','TableId':0,'TableKind':'PrimaryResult','TableName':'PrimaryResult','Columns':[{'ColumnName':'MyColumn','ColumnType':'string'}],'Rows':[['a']]},{'FrameType':'DataSetCompletion','HasErrors':false,'Cancelled':false}]".Replace("\'", "\""));
        }
    }
}