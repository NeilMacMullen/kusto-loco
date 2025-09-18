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
        var eng = new BabyKustoEngine(new SystemConsole(), new KustoSettingsProvider());
        var query = """
                    Resources | project X
                    | render linechart
                    """;

        var v = eng.GetVisualizationState(query);
        v.ChartType.Should().Be("linechart");
    }


    [TestMethod]
    public void RenderWithProperties()
    {
        var eng = new BabyKustoEngine(new SystemConsole(), new KustoSettingsProvider());
        var query = """
                    Resources | project X
                    | render scatterchart with 
                    (
                    kind=map
                    title="hello"
                    )
                    """;
        var v = eng.GetVisualizationState(query);
        v.ChartType.Should().Be("scatterchart");
        v.PropertyOr("kind", string.Empty).Should().Be("map");
        v.PropertyOr("title", string.Empty).Should().Be("hello");
    }

    [TestMethod]
    public void RenderMapDirect()
    {
        var eng = new BabyKustoEngine(new SystemConsole(), new KustoSettingsProvider());
        var query = """
                    Resources | project X
                    | render map
                    """;
        var v = eng.GetVisualizationState(query);
        v.ChartType.Should().Be("map");
    }
}
