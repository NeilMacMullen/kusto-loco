// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using BabyKusto.Core;
using BabyKusto.Server.Contract;
using BabyKusto.Server.Service;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BabyKusto.Server.Tests
{
    public class ManagementEndpointHelperTests
    {
        [Fact]
        public void ShowVersion_Works()
        {
            // Arrange
            var sut = CreateInstance();
            var body = new KustoApiMgmtRequestBody
            {
                Csl = ".show version",
            };

            // Act
            var result = sut.Process(body);

            // Assert
            result.Should().NotBeNull();
            result.Tables.Count.Should().Be(1);

            var table = result.Tables[0];
            table.TableName.Should().Be("Table_0");

            var columns = string.Join(",", table.Columns.Select(c => $"{c.ColumnName}:{c.DataType}:{c.ColumnType}"));
            columns.Should().Be("BuildVersion:String:,BuildTime:DateTime:,ServiceType:String:,ProductVersion:String:,ServiceOffering:String:");

            table.Rows.Count().Should().Be(1);
            var row = table.Rows[0];
            var values = string.Join(",", row.Select(v => v is null ? "null" : v.ToJsonString()));
            values.Should().Be("\"0.0.0\",\"2022-10-07T23:00:00Z\",\"Engine\",\"0.0.0\",\"{\\u0022Type\\u0022:\\u0022Azure Data Explorer\\u0022}\"");
        }

        [Fact]
        public void ShowDatabases_Works()
        {
            // Arrange
            var sut = CreateInstance();
            var body = new KustoApiMgmtRequestBody
            {
                Csl = ".show databases",
            };

            // Act
            var result = sut.Process(body);

            // Assert
            result.Should().NotBeNull();
            result.Tables.Count.Should().Be(1);

            var table = result.Tables[0];
            table.TableName.Should().Be("Table_0");

            var columns = string.Join(",", table.Columns.Select(c => $"{c.ColumnName}:{c.DataType}:{c.ColumnType}"));
            columns.Should().Be("DatabaseName:String:string,PersistentStorage:String:string,Version:String:string,IsCurrent:Boolean:bool,DatabaseAccessMode:String:string,PrettyName:String:string,ReservedSlot1:Boolean:bool,DatabaseId:Guid:guid,InTransitionTo:String:string");

            table.Rows.Count().Should().Be(1);
            var row = table.Rows[0];
            var values = string.Join(",", row.Select(v => v is null ? "null" : v.ToJsonString()));
            values.Should().Be("\"BabyKusto\",\"\",\"v1.0\",false,\"ReadWrite\",null,null,\"fe60d09c-de8e-4832-9bd3-ee3f5a723a26\",\"\"");
        }

        [Fact]
        public void ShowSchemaAsJson1_Works()
        {
            // Arrange
            var sut = CreateInstance();
            var body = new KustoApiMgmtRequestBody
            {
                Csl = ".show schema as json",
            };

            // Act
            var result = sut.Process(body);

            // Assert
        }

        [Fact]
        public void ShowSchemaAsJson2_Works()
        {
            // Arrange
            var sut = CreateInstance();
            var body = new KustoApiMgmtRequestBody
            {
                Csl = ".show databases  schema as json",
            };

            // Act
            var result = sut.Process(body);

            // Assert
        }

        [Fact]
        public void ShowSchemaAsJson3_Works()
        {
            // Arrange
            var sut = CreateInstance();
            var body = new KustoApiMgmtRequestBody
            {
                Csl = ".show databases (['BabyKusto']) schema as json",
            };

            // Act
            var result = sut.Process(body);

            // Assert
        }

        private static ManagementEndpointHelper CreateInstance()
        {
            var options = Options.Create(
                new BabyKustoServerOptions
                {
                    DatabaseId = "fe60d09c-de8e-4832-9bd3-ee3f5a723a26", // Make it deterministic
                });
            var tablesProvider = new ConstTablesProvider();
            var state = new BabyKustoServerState(options, tablesProvider);
            return new ManagementEndpointHelper(state);
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