// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyKusto.Core;
using BabyKusto.Server.Contract;
using BabyKusto.Server.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace BabyKusto.Server.Tests
{
    public class QueryV2EndpointHelperTests
    {
        [Fact]
        public async Task SpecialDoubles_Works()
        {
            // Act
            var result = await ExecuteQueryAsync(
                csl: "print posInf=1.0/0, negInf=-1.0/0, nan=0.0/0, longDivZero=0/0");

            // Assert
            result.Should().Be("[{\"FrameType\":\"DataSetHeader\",\"IsProgressive\":false,\"Version\":\"v2.0\"},{\"FrameType\":\"DataTable\",\"TableId\":0,\"TableKind\":\"PrimaryResult\",\"TableName\":\"PrimaryResult\",\"Columns\":[{\"ColumnName\":\"posInf\",\"ColumnType\":\"real\"},{\"ColumnName\":\"negInf\",\"ColumnType\":\"real\"},{\"ColumnName\":\"nan\",\"ColumnType\":\"real\"},{\"ColumnName\":\"longDivZero\",\"ColumnType\":\"long\"}],\"Rows\":[[\"Infinity\",\"-Infinity\",\"NaN\",null]]},{\"FrameType\":\"DataSetCompletion\",\"HasErrors\":false,\"Cancelled\":false}]");
        }

        [Fact]
        public async Task Dynamic_Works()
        {
            // Act
            var result = await ExecuteQueryAsync(
                csl: "print obj=dynamic({\"a\":{\"b\":2}}), arr=dynamic([1,2,3])");

            // Assert
            result.Should().Be("[{\"FrameType\":\"DataSetHeader\",\"IsProgressive\":false,\"Version\":\"v2.0\"},{\"FrameType\":\"DataTable\",\"TableId\":0,\"TableKind\":\"PrimaryResult\",\"TableName\":\"PrimaryResult\",\"Columns\":[{\"ColumnName\":\"obj\",\"ColumnType\":\"dynamic\"},{\"ColumnName\":\"arr\",\"ColumnType\":\"dynamic\"}],\"Rows\":[[\"{\\u0022a\\u0022:{\\u0022b\\u0022:2}}\",\"[1,2,3]\"]]},{\"FrameType\":\"DataSetCompletion\",\"HasErrors\":false,\"Cancelled\":false}]");
        }

        [Fact]
        public async Task Visualization_Works()
        {
            // Act
            var result = await ExecuteQueryAsync(csl: @"
datatable(lon:real,lat:real, name:string)[
    -122.3493, 47.6205, 'Space Needle',
    -122.3402, 47.6089, 'Pike Place Market',
]
| render scatterchart with (kind=map)
");

            // Assert
            result.Should().Be("[{\"FrameType\":\"DataSetHeader\",\"IsProgressive\":false,\"Version\":\"v2.0\"},{\"FrameType\":\"DataTable\",\"TableId\":0,\"TableKind\":\"PrimaryResult\",\"TableName\":\"PrimaryResult\",\"Columns\":[{\"ColumnName\":\"lon\",\"ColumnType\":\"real\"},{\"ColumnName\":\"lat\",\"ColumnType\":\"real\"},{\"ColumnName\":\"name\",\"ColumnType\":\"string\"}],\"Rows\":[[-122.3493,47.6205,\"Space Needle\"],[-122.3402,47.6089,\"Pike Place Market\"]]},{\"FrameType\":\"DataTable\",\"TableId\":1,\"TableKind\":\"QueryProperties\",\"TableName\":\"QueryProperties\",\"Columns\":[{\"ColumnName\":\"TableIndex\",\"ColumnType\":\"long\"},{\"ColumnName\":\"Type\",\"ColumnType\":\"string\"},{\"ColumnName\":\"Value\",\"ColumnType\":\"string\"}],\"Rows\":[[0,\"Visualization\",\"{\\u0022Visualization\\u0022:\\u0022scatterchart\\u0022,\\u0022Kind\\u0022:\\u0022map\\u0022,\\u0022YSplit\\u0022:null,\\u0022Legend\\u0022:null,\\u0022XAxis\\u0022:null,\\u0022YAxis\\u0022:null,\\u0022Title\\u0022:null,\\u0022XColumn\\u0022:null,\\u0022Series\\u0022:null,\\u0022YColumns\\u0022:null,\\u0022AnomalyColumns\\u0022:null,\\u0022XTitle\\u0022:null,\\u0022YTitle\\u0022:null,\\u0022Accumulate\\u0022:false,\\u0022IsQuerySorted\\u0022:false,\\u0022Ymin\\u0022:\\u0022NaN\\u0022,\\u0022Ymax\\u0022:\\u0022NaN\\u0022,\\u0022Xmin\\u0022:null,\\u0022Xmax\\u0022:null}\"]]},{\"FrameType\":\"DataSetCompletion\",\"HasErrors\":false,\"Cancelled\":false}]");
        }

        private static async Task<string> ExecuteQueryAsync(string csl, params ITableSource[] tables)
        {
            // Arrange
            var sut = CreateInstance();
            var body = new KustoApiQueryRequestBody
            {
                Csl = csl,
            };
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Act
            await sut.Process(body, context);

            responseBody.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(responseBody, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static QueryV2EndpointHelper CreateInstance(params ITableSource[] tables)
        {
            var options = Options.Create(
                new BabyKustoServerOptions
                {
                    DatabaseId = "fe60d09c-de8e-4832-9bd3-ee3f5a723a26", // Make it deterministic
                });
            var tablesProvider = new ConstTablesProvider(tables);
            var state = new BabyKustoServerState(options, tablesProvider);
            return new QueryV2EndpointHelper(state);
        }

        private class ConstTablesProvider : ITablesProvider
        {
            private readonly ITableSource[] _tables;

            public ConstTablesProvider(params ITableSource[] tables)
            {
                _tables = tables ?? throw new System.ArgumentNullException(nameof(tables));
            }

            public List<ITableSource> GetTables()
            {
                return _tables.ToList();
            }
        }
    }
}