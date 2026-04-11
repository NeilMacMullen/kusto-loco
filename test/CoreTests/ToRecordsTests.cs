//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using KustoLoco.Core;
using LogSetup;
using NLog;
using Xunit;

namespace KustoExecutionEngine.Core.Tests;

/// <summary>
///     Tests for the KustoQueryResult.ToRecords method
/// </summary>
public class ToRecordsTests
{
    public ToRecordsTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    #region Test Record Types

    public class SimpleRecord
    {
        public string Name { get; set; } = string.Empty;
        public long Age { get; set; }
        public double Score { get; set; }
    }

    public class RecordWithNullableTypes
    {
        public string? Name { get; set; }
        public long? Age { get; set; }
        public double? Score { get; set; }
    }

    public class RecordWithJsonPropertyName
    {
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("user_age")]
        public int UserAge { get; set; }
    }

    public class RecordWithBool
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RecordWithDateTime
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RecordWithTimeSpan
    {
        public string Name { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
    }

    public class RecordWithGuid
    {
        public string Name { get; set; } = string.Empty;
        public Guid Id { get; set; }
    }

    public enum Status
    {
        Active,
        Inactive,
        Pending
    }

    public class RecordWithPartialMatch
    {
        public string Name { get; set; } = string.Empty;
        // This property won't match any column
        public string ExtraProperty { get; set; } = "default";
    }

    /// <summary>
    ///     Simple record with only A and B properties - used to test
    ///     scenarios where query returns extra columns (like C) that should be ignored
    /// </summary>
    public class SimpleABRecord
    {
        public long A { get; set; }
        public long B { get; set; }
    }

    #endregion

    [Fact]
    public void ToRecords_BasicTypes_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Age:long, Score:real)
            [
                'Alice', 30, 95.5,
                'Bob', 25, 88.0,
                'Charlie', 35, 92.3
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<SimpleRecord>();

        // Assert
        records.Count.Should().Be(3);

        var recordList = new List<SimpleRecord>(records);
        recordList[0].Name.Should().Be("Alice");
        recordList[0].Age.Should().Be(30);
        recordList[0].Score.Should().Be(95.5);

        recordList[1].Name.Should().Be("Bob");
        recordList[1].Age.Should().Be(25);
        recordList[1].Score.Should().Be(88.0);

        recordList[2].Name.Should().Be("Charlie");
        recordList[2].Age.Should().Be(35);
        recordList[2].Score.Should().Be(92.3);
    }

    [Fact]
    public void ToRecords_WithMaxParameter_LimitsResults()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Age:long, Score:real)
            [
                'Alice', 30, 95.5,
                'Bob', 25, 88.0,
                'Charlie', 35, 92.3,
                'Diana', 28, 90.0
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<SimpleRecord>(2);

        // Assert
        records.Count.Should().Be(2);

        var recordList = new List<SimpleRecord>(records);
        recordList[0].Name.Should().Be("Alice");
        recordList[1].Name.Should().Be("Bob");
    }

    [Fact]
    public void ToRecords_EmptyResult_ReturnsEmptyCollection()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Age:long)
            [
                'Alice', 30
            ]
            | where Age > 100
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<SimpleRecord>();

        // Assert
        records.Count.Should().Be(0);
    }

    [Fact]
    public void ToRecords_CaseInsensitivePropertyMatching()
    {
        // Arrange
        var context = new KustoQueryContext();
        // Column names have different casing than property names
        var query = """
            datatable(name:string, AGE:long, SCORE:real)
            [
                'Alice', 30, 95.5
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<SimpleRecord>();

        // Assert
        records.Count.Should().Be(1);

        var record = new List<SimpleRecord>(records)[0];
        record.Name.Should().Be("Alice");
        record.Age.Should().Be(30);
        record.Score.Should().Be(95.5);
    }

    [Fact]
    public void ToRecords_WithJsonPropertyNameAttribute_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(user_name:string, user_age:int)
            [
                'Alice', 30,
                'Bob', 25
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<RecordWithJsonPropertyName>();

        // Assert
        records.Count.Should().Be(2);

        var recordList = new List<RecordWithJsonPropertyName>(records);
        recordList[0].UserName.Should().Be("Alice");
        recordList[0].UserAge.Should().Be(30);
        recordList[1].UserName.Should().Be("Bob");
        recordList[1].UserAge.Should().Be(25);
    }

    [Fact]
    public void ToRecords_BooleanType_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, IsActive:bool)
            [
                'Alice', true,
                'Bob', false
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<RecordWithBool>();

        // Assert
        records.Count.Should().Be(2);

        var recordList = new List<RecordWithBool>(records);
        recordList[0].Name.Should().Be("Alice");
        recordList[0].IsActive.Should().BeTrue();
        recordList[1].Name.Should().Be("Bob");
        recordList[1].IsActive.Should().BeFalse();
    }

    [Fact]
    public void ToRecords_DateTimeType_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, CreatedAt:datetime)
            [
                'Alice', datetime(2024-01-15),
                'Bob', datetime(2024-06-20)
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<RecordWithDateTime>();

        // Assert
        records.Count.Should().Be(2);

        var recordList = new List<RecordWithDateTime>(records);
        recordList[0].Name.Should().Be("Alice");
        recordList[0].CreatedAt.Should().Be(new DateTime(2024, 1, 15));
        recordList[1].Name.Should().Be("Bob");
        recordList[1].CreatedAt.Should().Be(new DateTime(2024, 6, 20));
    }

    [Fact]
    public void ToRecords_TimeSpanType_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Duration:timespan)
            [
                'Task1', 1h,
                'Task2', 30m
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<RecordWithTimeSpan>();

        // Assert
        records.Count.Should().Be(2);

        var recordList = new List<RecordWithTimeSpan>(records);
        recordList[0].Name.Should().Be("Task1");
        recordList[0].Duration.Should().Be(TimeSpan.FromHours(1));
        recordList[1].Name.Should().Be("Task2");
        recordList[1].Duration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void ToRecords_GuidType_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var expectedGuid1 = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var expectedGuid2 = Guid.Parse("87654321-4321-4321-4321-210987654321");
        var query = $"""
            datatable(Name:string, Id:guid)
            [
                'Item1', guid({expectedGuid1}),
                'Item2', guid({expectedGuid2})
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<RecordWithGuid>();

        // Assert
        records.Count.Should().Be(2);

        var recordList = new List<RecordWithGuid>(records);
        recordList[0].Name.Should().Be("Item1");
        recordList[0].Id.Should().Be(expectedGuid1);
        recordList[1].Name.Should().Be("Item2");
        recordList[1].Id.Should().Be(expectedGuid2);
    }

    [Fact]
    public void ToRecords_PartialColumnMatch_MapsMatchingPropertiesOnly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Age:long)
            [
                'Alice', 30
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<RecordWithPartialMatch>();

        // Assert
        records.Count.Should().Be(1);

        var record = new List<RecordWithPartialMatch>(records)[0];
        record.Name.Should().Be("Alice");
        // ExtraProperty should retain its default value since no column matches
        record.ExtraProperty.Should().Be("default");
    }

    [Fact]
    public void ToRecords_ExtraColumnsInResult_IgnoresUnmappedColumns()
    {
        // Arrange - query returns more columns than the record type has properties
        var context = new KustoQueryContext();
        var query = """
            datatable(B:long, A:long, C:long)
            [
                20, 10, 30,
                200, 100, 300
            ]
            """;

        // Act - SimpleABRecord only has A and B properties, C should be ignored
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<SimpleABRecord>();

        // Assert
        records.Count.Should().Be(2);

        var recordList = new List<SimpleABRecord>(records);
        // Verify A and B are mapped correctly regardless of column order
        recordList[0].A.Should().Be(10);
        recordList[0].B.Should().Be(20);
        recordList[1].A.Should().Be(100);
        recordList[1].B.Should().Be(200);
    }

    [Fact]
    public void ToRecords_TypeConversion_IntToLong()
    {
        // Arrange
        var context = new KustoQueryContext();
        // Kusto int to C# long
        var query = """
            datatable(Name:string, Age:int, Score:real)
            [
                'Alice', 30, 95.5
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<SimpleRecord>();

        // Assert
        records.Count.Should().Be(1);

        var record = new List<SimpleRecord>(records)[0];
        record.Age.Should().Be(30);
    }

    [Fact]
    public void ToRecords_RoundTrip_DataIntegrity()
    {
        // Arrange - create data, query it, convert to records
        var originalData = ImmutableArray.Create(
            new SimpleRecord { Name = "Test1", Age = 100, Score = 99.9 },
            new SimpleRecord { Name = "Test2", Age = 200, Score = 88.8 }
        );

        var context = new KustoQueryContext();
        context.WrapDataIntoTable("testdata", originalData);

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading("testdata");
        var records = result.ToRecords<SimpleRecord>();

        // Assert
        records.Count.Should().Be(2);

        var recordList = new List<SimpleRecord>(records);
        recordList[0].Name.Should().Be("Test1");
        recordList[0].Age.Should().Be(100);
        recordList[0].Score.Should().Be(99.9);
        recordList[1].Name.Should().Be("Test2");
        recordList[1].Age.Should().Be(200);
        recordList[1].Score.Should().Be(88.8);
    }

    [Fact]
    public void ToRecords_WithProjection_MapsProjectedColumns()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(FirstName:string, LastName:string, Age:long, Score:real)
            [
                'Alice', 'Smith', 30, 95.5
            ]
            | project Name = strcat(FirstName, ' ', LastName), Age, Score
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var records = result.ToRecords<SimpleRecord>();

        // Assert
        records.Count.Should().Be(1);

        var record = new List<SimpleRecord>(records)[0];
        record.Name.Should().Be("Alice Smith");
        record.Age.Should().Be(30);
        record.Score.Should().Be(95.5);
    }

    [Fact]
    public void ToRecords_MultipleCallsSameType_UsesCachedPropertyInfo()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query1 = """
            datatable(Name:string, Age:long, Score:real)
            [
                'Alice', 30, 95.5
            ]
            """;
        var query2 = """
            datatable(Name:string, Age:long, Score:real)
            [
                'Bob', 25, 88.0
            ]
            """;

        // Act - multiple calls should use cached property mappings
        var result1 = context.RunQueryWithoutDemandBasedTableLoading(query1);
        var records1 = result1.ToRecords<SimpleRecord>();

        var result2 = context.RunQueryWithoutDemandBasedTableLoading(query2);
        var records2 = result2.ToRecords<SimpleRecord>();

        // Assert
        records1.Count.Should().Be(1);
        records2.Count.Should().Be(1);

        new List<SimpleRecord>(records1)[0].Name.Should().Be("Alice");
        new List<SimpleRecord>(records2)[0].Name.Should().Be("Bob");
    }
}
