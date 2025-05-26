using ScottPlot;

namespace lokqlDx;

public interface IScottPlotHost
{
    Plot GetPlot(bool reset);
    void FinishUpdate();
    void CopyToClipboard();
}
