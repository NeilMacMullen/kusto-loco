using KustoLoco.Rendering.ScottPlot;
using ScottPlot;
using ScottPlot.Plottables;
using System.Globalization;
using System.Text;

namespace LokqlDx.Views;

public class PopupSupport
{
    private readonly Plot _plot;

    public PopupSupport(Plot plot)
    {
        _plot = plot;
    }
    private PopupResult TryPopupFromBar(Coordinates rawCoordinates)
    {
        var inverted = new Coordinates(rawCoordinates.Y, rawCoordinates.X);
        var bestResult = PopupResult.None;
        foreach (var bar in _plot.GetPlottables<BarPlot>())
        {
            var bars = bar.Bars;
            foreach (var b in bars)
            {
                var invert = b.Orientation == Orientation.Horizontal;
                var coordinates = invert
                    ? inverted
                    : rawCoordinates;
                var barLeft = b.Position - b.Size / 2;
                var barRight = b.Position + b.Size / 2;

                if (coordinates.X < barLeft)
                    continue;
                if (coordinates.X > barRight)
                    continue;
                if (coordinates.Y < b.ValueBase)
                    continue;
                if (coordinates.Y > b.Value)
                    continue;

                //we have a candidate
                return new PopupResult(bar.LegendText, b.Position, b.ValueBase, b.Value, invert);
            }
        }
        return bestResult;
    }
    private PopupResult TryPopupFromScatter(Coordinates coordinates)
    {
        foreach (var scatter in _plot.GetPlottables<Scatter>())
        {
            var near = scatter.GetNearest(coordinates, _plot.LastRender);
            if (near.IsReal) return new PopupResult(scatter.LegendText, near.X, near.Y, near.Y, false);
        }

        return PopupResult.None;
    }

    private string GetLabel(IAxis axis, double v)
    {
        //TODO - temporary implementation until fixed in ScottPlot
        try
        {
            if (axis.TickGenerator is FixedDateTimeAutomatic)
                return DateTime.FromOADate(v).ToString(); //TODO - get string format from kusto settings
        }
        catch 
        {
            //It's possible that the value is not a valid OLE Automation date
        }

        //TODO - realy this needs to lookup the original value from the data source
        //so that we can get non-numeric values like strings
        return v.ToString(CultureInfo.InvariantCulture);
    }
    
    private string GetXLabel(double x) => GetLabel(_plot.Axes.Bottom, x);

    private string GetYLabel(double y) => GetLabel(_plot.Axes.Left, y);

    internal PopupResult TryPopup(Coordinates coordinates)
    {
        var popup = TryPopupFromScatter(coordinates);
        if (!popup.IsValid)
            popup = TryPopupFromBar(coordinates);
        return popup;

    }

    internal string GetPopupContents(PopupResult popup)
    {
        var sb = new StringBuilder();
        sb.AppendLine(popup.Series);
        sb.AppendLine(popup.InvertAxes ? GetYLabel(popup.X) : GetXLabel(popup.X));

        sb.AppendLine(popup.Y == popup.V
            ? GetYLabel(popup.Y)
            : (popup.V - popup.Y).ToString(CultureInfo.InvariantCulture));
        return sb.ToString();

    }

    public string GetBasicPositionInfo(Coordinates coordinates)
    {
        return $"X:{GetXLabel(coordinates.X)} Y:{GetYLabel(coordinates.Y)}";
    }
}
