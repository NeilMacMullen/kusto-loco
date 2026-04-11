//
// Licensed under the MIT License.

using System;
using System.Data;
using System.Linq;
using AwesomeAssertions;
using KustoLoco.Core;
using LogSetup;
using NLog;
using Xunit;

namespace KustoExecutionEngine.Core.Tests;

/// <summary>
///     Tests for the KustoQueryResult.ToDataTable and ToDataTableOrError methods
/// </summary>
public class ToDataTableTests
{
    public ToDataTableTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    [Fact]
    public void ToDataTable_BasicTypes_CreatesCorrectColumns()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Age:long, Score:real, IsActive:bool)
            [
                'Alice', 30, 95.5, true
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns.Count.Should().Be(4);
        dataTable.Columns["Name"]!.DataType.Should().Be(typeof(string));
        dataTable.Columns["Age"]!.DataType.Should().Be(typeof(long));
        dataTable.Columns["Score"]!.DataType.Should().Be(typeof(double));
        dataTable.Columns["IsActive"]!.DataType.Should().Be(typeof(bool));
    }

    [Fact]
    public void ToDataTable_BasicTypes_CreatesCorrectRows()
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
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Rows.Count.Should().Be(3);

        dataTable.Rows[0]["Name"].Should().Be("Alice");
        dataTable.Rows[0]["Age"].Should().Be(30L);
        dataTable.Rows[0]["Score"].Should().Be(95.5);

        dataTable.Rows[1]["Name"].Should().Be("Bob");
        dataTable.Rows[1]["Age"].Should().Be(25L);
        dataTable.Rows[1]["Score"].Should().Be(88.0);

        dataTable.Rows[2]["Name"].Should().Be("Charlie");
        dataTable.Rows[2]["Age"].Should().Be(35L);
        dataTable.Rows[2]["Score"].Should().Be(92.3);
    }

    [Fact]
    public void ToDataTable_WithMaxRows_LimitsResults()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Value:long)
            [
                'A', 1,
                'B', 2,
                'C', 3,
                'D', 4,
                'E', 5
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable(3);

        // Assert
        dataTable.Rows.Count.Should().Be(3);
        dataTable.Rows[0]["Name"].Should().Be("A");
        dataTable.Rows[1]["Name"].Should().Be("B");
        dataTable.Rows[2]["Name"].Should().Be("C");
    }

    [Fact]
    public void ToDataTable_EmptyResult_ReturnsEmptyDataTable()
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
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Rows.Count.Should().Be(0);
        // Columns should still be defined
        dataTable.Columns.Count.Should().Be(2);
        dataTable.Columns["Name"]!.DataType.Should().Be(typeof(string));
        dataTable.Columns["Age"]!.DataType.Should().Be(typeof(long));
    }

    [Fact]
    public void ToDataTable_DateTimeType_MapsCorrectly()
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
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns["CreatedAt"]!.DataType.Should().Be(typeof(DateTime));
        dataTable.Rows[0]["CreatedAt"].Should().Be(new DateTime(2024, 1, 15));
        dataTable.Rows[1]["CreatedAt"].Should().Be(new DateTime(2024, 6, 20));
    }

    [Fact]
    public void ToDataTable_TimeSpanType_MapsCorrectly()
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
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns["Duration"]!.DataType.Should().Be(typeof(TimeSpan));
        dataTable.Rows[0]["Duration"].Should().Be(TimeSpan.FromHours(1));
        dataTable.Rows[1]["Duration"].Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void ToDataTable_GuidType_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var expectedGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var query = $"""
            datatable(Name:string, Id:guid)
            [
                'Item1', guid({expectedGuid})
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns["Id"]!.DataType.Should().Be(typeof(Guid));
        dataTable.Rows[0]["Id"].Should().Be(expectedGuid);
    }

    [Fact]
    public void ToDataTable_IntType_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Count:int)
            [
                'Item1', 42,
                'Item2', 100
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns["Count"]!.DataType.Should().Be(typeof(int));
        dataTable.Rows[0]["Count"].Should().Be(42);
        dataTable.Rows[1]["Count"].Should().Be(100);
    }

    [Fact]
    public void ToDataTable_DecimalType_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Price:decimal)
            [
                'Item1', decimal(123.45),
                'Item2', decimal(999.99)
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns["Price"]!.DataType.Should().Be(typeof(decimal));
        dataTable.Rows[0]["Price"].Should().Be(123.45m);
        dataTable.Rows[1]["Price"].Should().Be(999.99m);
    }

    [Fact]
    public void ToDataTable_WithProjection_ReflectsProjectedColumns()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(FirstName:string, LastName:string, Age:long)
            [
                'Alice', 'Smith', 30
            ]
            | project FullName = strcat(FirstName, ' ', LastName), Age
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns.Count.Should().Be(2);
        dataTable.Columns[0].ColumnName.Should().Be("FullName");
        dataTable.Columns[1].ColumnName.Should().Be("Age");
        dataTable.Rows[0]["FullName"].Should().Be("Alice Smith");
        dataTable.Rows[0]["Age"].Should().Be(30L);
    }

    [Fact]
    public void ToDataTable_ColumnOrderPreserved()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Z:string, A:string, M:string)
            [
                'z1', 'a1', 'm1'
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns[0].ColumnName.Should().Be("Z");
        dataTable.Columns[1].ColumnName.Should().Be("A");
        dataTable.Columns[2].ColumnName.Should().Be("M");
    }

    [Fact]
    public void ToDataTable_MaxRowsGreaterThanActual_ReturnsAllRows()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string)
            [
                'A', 'B', 'C'
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable(100); // Ask for 100, but only 3 exist

        // Assert
        dataTable.Rows.Count.Should().Be(3);
    }

    [Fact]
    public void ToDataTable_MaxRowsZero_ReturnsEmptyDataTable()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string)
            [
                'A', 'B', 'C'
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable(0);

        // Assert
        dataTable.Rows.Count.Should().Be(0);
        dataTable.Columns.Count.Should().Be(1); // Columns still present
    }

    #region ToDataTableOrError Tests

    [Fact]
    public void ToDataTableOrError_NoError_ReturnsNormalDataTable()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Age:long)
            [
                'Alice', 30,
                'Bob', 25
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTableOrError();

        // Assert
        dataTable.Columns.Count.Should().Be(2);
        dataTable.Rows.Count.Should().Be(2);
        dataTable.Columns[0].ColumnName.Should().Be("Name");
        dataTable.Columns[1].ColumnName.Should().Be("Age");
    }

    [Fact]
    public void ToDataTableOrError_WithError_ReturnsErrorDataTable()
    {
        // Arrange
        var context = new KustoQueryContext();
        // Invalid query that will produce an error
        var query = "invalid_query_syntax!!!";

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);

        // The result should have an error
        result.Error.Should().NotBeEmpty();

        var dataTable = result.ToDataTableOrError();

        // Assert
        dataTable.Columns.Count.Should().Be(1);
        dataTable.Columns[0].ColumnName.Should().Be("ERROR");
        dataTable.Rows.Count.Should().Be(1);
        dataTable.Rows[0]["ERROR"].Should().Be(result.Error);
    }

    [Fact]
    public void ToDataTableOrError_WithMaxRows_LimitsResults()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string)
            [
                'A', 'B', 'C', 'D', 'E'
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTableOrError(2);

        // Assert
        dataTable.Rows.Count.Should().Be(2);
    }

    [Fact]
    public void ToDataTableOrError_ErrorIgnoresMaxRows()
    {
        // Arrange - When there's an error, maxRows shouldn't affect the error table
        var context = new KustoQueryContext();
        var query = "completely_invalid!!!";

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTableOrError(0); // Even with 0 maxRows

        // Assert - Error table should still have 1 row with the error
        dataTable.Columns[0].ColumnName.Should().Be("ERROR");
        dataTable.Rows.Count.Should().Be(1);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ToDataTable_CanBeUsedWithLinq()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Name:string, Age:long)
            [
                'Alice', 30,
                'Bob', 25,
                'Charlie', 35
            ]
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable();

        // Use LINQ to filter the DataTable
        var over30 = dataTable.AsEnumerable()
            .Where(row => (long)row["Age"] > 28)
            .Select(row => (string)row["Name"])
            .ToList();

        // Assert
        over30.Count.Should().Be(2);
        over30.Should().Contain("Alice");
        over30.Should().Contain("Charlie");
    }

    [Fact]
    public void ToDataTable_AggregationQuery_MapsCorrectly()
    {
        // Arrange
        var context = new KustoQueryContext();
        var query = """
            datatable(Category:string, Value:long)
            [
                'A', 10,
                'A', 20,
                'B', 30,
                'B', 40,
                'B', 50
            ]
            | summarize Total = sum(Value), Count = count() by Category
            """;

        // Act
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        var dataTable = result.ToDataTable();

        // Assert
        dataTable.Columns.Count.Should().Be(3);
        dataTable.Rows.Count.Should().Be(2);

        // Find rows by category (order may vary)
        var rowA = dataTable.AsEnumerable().First(r => (string)r["Category"] == "A");
        var rowB = dataTable.AsEnumerable().First(r => (string)r["Category"] == "B");

        rowA["Total"].Should().Be(30L);
        rowA["Count"].Should().Be(2L);
        rowB["Total"].Should().Be(120L);
        rowB["Count"].Should().Be(3L);
    }

    #endregion
}
