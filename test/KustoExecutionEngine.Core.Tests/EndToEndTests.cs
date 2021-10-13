using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace KustoExecutionEngine.Core.Tests
{
    public class EndToEndTests
    {
        [Fact]
        public void JustTableReference_Works()
        {
            // Arrange
            var engine = new StirlingEngine();
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
        }

        [Fact]
        public void Project_Works()
        {
            // Arrange
            var engine = new StirlingEngine();
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
        }
    }
}