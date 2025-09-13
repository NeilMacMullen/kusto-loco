namespace KustoLoco.Rendering.ScottPlot;

public class RenderInfo
{
    public double MaxY;
    public double MinY;
    public bool HasYSpan => MaxY!=MinY;

    public RenderInfo InflateY(double d)
    {
        var span = MaxY - MinY;
        var extra = span * d;
        return new RenderInfo()
        {
            MaxY = MaxY + extra,
            MinY = MinY - extra
        };
    }
}
