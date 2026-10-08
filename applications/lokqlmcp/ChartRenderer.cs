using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.ScottPlot;
using ScottPlot;

namespace LokqlMcp;

public static class ChartRenderer
{
    public static bool IsMap(KustoQueryResult result) =>
        string.Equals(result.Visualization.PropertyOr("kind", string.Empty), "map",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Returns PNG bytes, or null if the result can't be rendered as a chart.
    /// </summary>
    public static byte[]? TryRender(KustoQueryResult result, KustoSettingsProvider settings,
        out string? error)
    {
        error = null;
        if (!result.IsChart || IsMap(result))
            return null;
        try
        {
            return ScottPlotKustoResultRenderer.RenderToImage(result, ImageFormat.Png, 1000, 600, settings);
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return null;
        }
    }
}
