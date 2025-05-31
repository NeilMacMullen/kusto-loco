using System.Globalization;
using System.Text;
using KustoLoco.Rendering.ScottPlot;
using ScottPlot;
using ScottPlot.Plottables;

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

    private string GetXLabel(double x)
    {
        //TODO - temporary implementation until fixed in ScottPlot
        if (_plot.Axes.Bottom.TickGenerator is FixedDateTimeAutomatic)
            return DateTime.FromOADate(x).ToString(); //TODO - get string format from kusto settings
        //TODO - realy this needs to lookup the original value from the data source
        //so that we can get non-numeric values like strings
        return x.ToString(CultureInfo.InvariantCulture);
    }

    private string GetYLabel(double y)
    {
        //TODO - temporary implementation until fixed in ScottPlot
        if (_plot.Axes.Left.TickGenerator is FixedDateTimeAutomatic)
            return DateTime.FromOADate(y).ToString();//TODO - get string format from kusto settings
        //TODO - realy this needs to lookup the original value from the data source
        //so that we can get non-numeric values like strings
        return y.ToString(CultureInfo.InvariantCulture);
    }

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
        if (popup.InvertAxes)
            sb.AppendLine(GetYLabel(popup.X));
        else
            sb.AppendLine(GetXLabel(popup.X));

        if (popup.Y == popup.V)
            sb.AppendLine(GetYLabel(popup.Y));
        else
            sb.AppendLine((popup.V - popup.Y).ToString(CultureInfo.InvariantCulture));
        return sb.ToString();

    }

    public string GetBasicPositionInfo(Coordinates coordinates)
    {
        return $"X:{GetXLabel(coordinates.X)} Y:{GetYLabel(coordinates.Y)}";
    }
}
