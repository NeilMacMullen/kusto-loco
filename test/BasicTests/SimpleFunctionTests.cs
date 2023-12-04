using FluentAssertions;
using KustoSupport;
using LogSetup;
using NLog;

namespace BasicTests;

[TestClass]
public class SimpleFunctionTests
{
    public SimpleFunctionTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    private static KustoQueryContext CreateContext()
    {
        return KustoQueryContext.WithFullDebug();
    }

    private async Task<string> LastLineOfResult(string query)
    {
        var context = CreateContext();
        var result = await context.RunQuery(query);
        //return the last line 
        return KustoFormatter.Tabulate(result.Results)
            .Trim()
            .Split(Environment.NewLine)
            .Last().Trim();
    }

    [TestMethod]
    public async Task TrimStart()
    {
        var query = @"print trim_start(@'a+','aaainnerbbba')";
        var result = await LastLineOfResult(query);
        result.Should().Be("innerbbba");
    }

    [TestMethod]
    public async Task TrimEnd()
    {
        var query = @"print trim_end(@'b+','baaainnerbbb')";
        var result = await LastLineOfResult(query);
        result.Should().Be("baaainner");
    }

    [TestMethod]
    public async Task Trim()
    {
        var query = @"print trim(@'a+','aaaainneraaaa')";
        var result = await LastLineOfResult(query);
        result.Should().Be("inner");
    }

    [TestMethod]
    public async Task Case()
    {
        var query = @" 
datatable(Size:int) [7] 
| extend S= case(Size <= 3, 'Small',                        
                 Size <= 10, 'Medium', 
                             'Large')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("Medium");
    }

    [TestMethod]
    public async Task CaseDefault()
    {
        var query = @" 
datatable(Size:int) [50] 
|extend S= case(Size <= 3, 'Small',                        
              Size <= 10, 'Medium', 
                          'Large')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("Large");
    }
}