namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword="heatmap_color")]
internal partial class HeatmapColorFunction
{
    private static string Impl(string type, double min, double max, double value) =>
        Heatmap.GetHeatmapColor(type, min, max, value);
}
