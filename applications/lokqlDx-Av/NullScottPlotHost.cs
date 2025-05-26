using ScottPlot;

namespace lokqlDx;

/// <summary>
/// An empty host that does nothing but which can be used as an initial value
/// </summary>
public class NullScottPlotHost : IScottPlotHost
{
    public Plot GetPlot(bool reset) => new();

    public void FinishUpdate()
    {
    }

    public void CopyToClipboard()
    {
    }
}
