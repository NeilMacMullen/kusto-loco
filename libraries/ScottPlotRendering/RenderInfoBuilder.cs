namespace KustoLoco.Rendering.ScottPlot;

public class RenderInfoBuilder
{
    private RenderInfo _ri = new RenderInfo();
    public RenderInfo Build()
    {
        return _ri;
    }

    public void HandleY(double[] orderedY)
    {
        foreach (var d in orderedY)
        {
            _ri.MaxY = Math.Max(_ri.MaxY, d);
            _ri.MinY = Math.Min(_ri.MinY, d);

        }
    }
}
