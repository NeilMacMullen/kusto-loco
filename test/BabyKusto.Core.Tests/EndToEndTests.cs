// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Core;
using BabyKusto.Core.Extensions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace KustoExecutionEngine.Core.Tests
{
    public class EndToEndTests
    {
        [Fact]
        public void JustTableReference_Works()
        {
            /*
            // Arrange
            var engine = new BabyKustoEngine();
            engine.AddGlobalTable(
                "MyTable",
                new[] { "a", "b" },
                new[]
                {
                    new Row(
                        new[]
                        {
                            new KeyValuePair<string, object?>("a", 1.0),
                            new KeyValuePair<string, object?>("b", 2.0),
                        }),
                    new Row(
                        new[]
                        {
                            new KeyValuePair<string, object?>("a", 1.5),
                            new KeyValuePair<string, object?>("b", 2.5),
                        }),
                });
            string query = "MyTable";

            // Act
            var result = engine.Evaluate(query) as ITabularSource;

            // Assert
            result.Should().NotBeNull();
            var row0 = result!.GetNextRow();
            var row0cols = row0!.ToArray();
            row0cols.Length.Should().Be(2);
            row0cols[0].Key.Should().Be("a");
            row0cols[0].Value.Should().Be(1.0);
            row0cols[1].Key.Should().Be("b");
            row0cols[1].Value.Should().Be(2.0);
            */
        }

        [Fact]
        public void Project_Works()
        {
            /*
            // Arrange
            var engine = new BabyKustoEngine();
            engine.AddGlobalTable(
                "MyTable",
                new[] { "a", "b" },
                new[]
                {
                    new Row(
                        new[]
                        {
                            new KeyValuePair<string, object?>("a", 1.0),
                            new KeyValuePair<string, object?>("b", 2.0),
                        }),
                    new Row(
                        new[]
                        {
                            new KeyValuePair<string, object?>("a", 1.5),
                            new KeyValuePair<string, object?>("b", 2.5),
                        }),
                });
            string query = @"
MyTable
| project a, (a+1), d=a+10.0";

            // Act
            var result = engine.Evaluate(query) as ITabularSource;

            // Assert
            result.Should().NotBeNull();
            var row0 = result!.GetNextRow();
            var row0cols = row0!.ToArray();
            row0cols.Length.Should().Be(3);

            row0cols[0].Key.Should().Be("a");
            row0cols[0].Value.Should().Be(1.0);

            row0cols[1].Key.Should().Be("Column1");
            row0cols[1].Value.Should().Be(2.0);

            row0cols[2].Key.Should().Be("d");
            row0cols[2].Value.Should().Be(11.0);
            */
        }

        [Fact]
        public void Summarize_NoByExpressions_Works()
        {
            // Arrange
            var engine = new BabyKustoEngine();
            engine.AddGlobalTable("MyTable", GetSampleData());
            var query = "MyTable | summarize count()";

            // Act
            var result = engine.Evaluate(query) as ITableSource;

            // Assert
            result.Should().NotBeNull();
            var dumped = result!.DumpToString();
            dumped.Should().Be(
                "count_; " + Environment.NewLine +
                "------------------" + Environment.NewLine +
                "5; " + Environment.NewLine);
        }

        [Fact]
        public void Example1_Works()
        {
            // Arrange
            var engine = new BabyKustoEngine();
            engine.AddGlobalTable("MyTable", GetSampleData());
            var query = @"
let c=100.0;
MyTable
| project frac=CounterValue/c, AppMachine, CounterName
| summarize avg(frac) by CounterName
| project CounterName, avgRoundedPercent=tolong(avg_frac*100)
";

            // Act
            var result = engine.Evaluate(query) as ITableSource;

            // Assert
            result.Should().NotBeNull();
            var dumped = result!.DumpToString();
            dumped.Should().Be(
                "CounterName; avgRoundedPercent; " + Environment.NewLine +
                "------------------" + Environment.NewLine +
                "cpu; 57; " + Environment.NewLine +
                "mem; 18; " + Environment.NewLine);
        }

        private static ITableSource GetSampleData()
        {
            return new InMemoryTableSource(
                new TableSchema(
                    new List<ColumnDefinition>()
                    {
                        new ColumnDefinition("AppMachine",   KustoValueKind.String),
                        new ColumnDefinition("CounterName",  KustoValueKind.String),
                        new ColumnDefinition("CounterValue", KustoValueKind.Real),
                    }),
                    new[]
                    {
                        new Column(new object?[] { "vm0", "vm0", "vm1", "vm1", "vm2" }),
                        new Column(new object?[] { "cpu", "mem", "cpu", "mem", "cpu" }),
                        new Column(new object?[] {  50.0,  30.0,  20.0,  5.0,   100.0 }),
                    });
        }
    }
}