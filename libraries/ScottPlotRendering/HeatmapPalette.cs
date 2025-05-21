using ScottPlot;

namespace KustoLoco.Rendering.ScottPlot;

public class HeatmapPalette : IPalette
{
    private readonly double _scale;
    private readonly IColormap _v;

    public HeatmapPalette(double scale,IColormap colorMap)
    {
        _scale = scale;
        _v = colorMap;
    }

    public Color GetColor(int index) => _v.GetColor(index / _scale);

    public Color[] Colors => throw new NotImplementedException();
    public string Name => "Heatmap";
    public string Description => "Heatmap";
}
