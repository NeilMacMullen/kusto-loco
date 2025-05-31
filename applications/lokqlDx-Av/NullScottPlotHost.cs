using KustoLoco.Core;
using KustoLoco.Core.Settings;

namespace lokqlDx;

/// <summary>
///     An empty host that does nothing but which can be used as an initial value
/// </summary>
public class NullScottPlotHost : IScottPlotHost
{
    public void CopyToClipboard()
    {
        //do nothing
    }

    public void RenderToDisplay(KustoQueryResult result, KustoSettingsProvider settings)
    {
        //do nothing
    }

    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight,
        KustoSettingsProvider settings) =>
        //return nothing
        [];
}
