using System.Collections.Immutable;
using FluentAssertions;
using KustoLoco.Core;
using KustoLoco.Core.DataSource.Columns;
using LogSetup;
using LogLevel = NLog.LogLevel;

namespace BasicTests;

[TestClass]
public class OperatorTests
{
    public OperatorTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    private static KustoQueryContext CreateContext()
    {
        return KustoQueryContext.WithFullDebug();
    }

    [TestMethod]
    public async Task GetSchema()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToImmutableArray();

        context.AddTableFromImmutableData("data", rows);
        var result = await context.RunTabularQueryAsync("data | getschema");
        var schema = KustoFormatter.Tabulate(result);
        Console.WriteLine(schema);
        schema.Should().Contain(nameof(Row.Name));
        schema.Should().Contain(nameof(Row.Value));
    }

    [TestMethod]
    public async Task IndexColumnsWorkLazily()
    {
        var context = CreateContext();

        var tableLength = 10;
        var table = TableBuilder.CreateEmpty("data", tableLength);

        var invocationCountForIndex = 0;
        var lazy = new SingleValueLambdaColumn<string?>(() =>
        {
            invocationCountForIndex++;
            return "a";
        }, tableLength);
        table = table
            .WithColumn("lazy", lazy);
        context.AddTable(table);
        await context.RunTabularQueryAsync("data | where  lazy == 'a' | count");
        invocationCountForIndex.Should().Be(1);
    }

    [TestMethod]
    public async Task IntOperationsAreCorrectlyPerformed()
    {
        //this may seem like an odd test but it checks for correct
        //handling of unexpected Kusto behaviour where any operation
        //on a pair of ints tends to end up with a long result
        //this risks putting the syntax analyser at odds with
        //arithmetic operations that might otherwise try to 
        //use C# rules

        var tableLength = 10;
        var context = CreateContext();


        var c1 = new SingleValueColumn<int?>(1, tableLength);
        var builder = TableBuilder.CreateEmpty("data", tableLength)
            .WithColumn("c1", c1)
            .WithColumn("c2", c1);
        context.AddTable(builder);
        //add.cs
        (await context.RunTabularQueryAsync("data | extend p=c1+c2"))
            .Error.Should()
            .BeEmpty();
        //subtract.cs
        (await context.RunTabularQueryAsync("data | extend p=c1-c2"))
            .Error.Should()
            .BeEmpty();

        //Multiply.cs
        (await context.RunTabularQueryAsync("data | extend p=c1*c2"))
            .Error.Should()
            .BeEmpty();

        //Divide.cs
        (await context.RunTabularQueryAsync("data | extend p=c1/c2"))
            .Error.Should()
            .BeEmpty();

        //Modulo.cs
        (await context.RunTabularQueryAsync("data | extend p=c1 % c2"))
            .Error.Should()
            .BeEmpty();

        //Binning
        (await context.RunTabularQueryAsync("data | extend p=bin(c1,c2)"))
            .Error.Should()
            .BeEmpty();

        (await context.RunTabularQueryAsync("data | summarize p=max(c1) by c2 "))
            .Error.Should()
            .BeEmpty();
    }
}