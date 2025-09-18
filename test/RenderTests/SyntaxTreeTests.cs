using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;

namespace RenderTests;

[TestClass]
public class SyntaxTreeTests
{
    [TestMethod]
    public void VisitRender()
    {
        var query = """
                    Resources | project X
                    |      render linechart
                    """;

        var (v,q) = KustoQueryContext.GetVisualizationState(query);
        v.ChartType.Should().Be("linechart");
        q.Should().Be("""
                      Resources | project X
                      """);
    }


    [TestMethod]
    public void RenderWithProperties()
    {
        var query = """
                    Resources | project X
                    | render scatterchart with 
                    (
                    kind=map
                    title="hello"
                    )
                    """;
        var (v, q) = KustoQueryContext.GetVisualizationState(query);
        v.ChartType.Should().Be("scatterchart");
        v.PropertyOr("kind", string.Empty).Should().Be("map");
        v.PropertyOr("title", string.Empty).Should().Be("hello");
        q.Should().Be("""
                      Resources | project X
                      """);
    }

    [TestMethod]
    public void RenderMapDirect()
    {
        var query = """
                    Resources | project X
                    | render map
                    """;
        var (v, q) = KustoQueryContext.GetVisualizationState(query);
        v.ChartType.Should().Be("map");
        q.Should().Be("""
                      Resources | project X
                      """);
    }


    [TestMethod]
    public void WithWhere()
    {
        var query = """
                    Resources
                    | summarize count() by type
                    | where count > 1
                    | render columnchart
                    """;
        var (v, q) = KustoQueryContext.GetVisualizationState(query);
        v.ChartType.Should().Be("columnchart");
        q.Should().Be("""
                      Resources
                      | summarize count() by type
                      | where count > 1
                      """);
    }
}
