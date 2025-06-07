using KustoLoco.Core;
using KustoLoco.Core.Settings;

namespace lokqlDx;

public interface IScottPlotHost
{
    void CopyToClipboard();
    void RenderToDisplay(KustoQueryResult result, KustoSettingsProvider settings);
    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight, KustoSettingsProvider settings);
}
