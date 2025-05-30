using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using KustoLoco.Rendering.ScottPlot;
using NotNullStrings;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;

namespace LokqlDx.Views;

public partial class ChartView : UserControl
{
    private Crosshair Crosshair;

    private string LastPopup = string.Empty;

    public ChartView()
    {
        InitializeComponent();
        Crosshair = PlotControl.Plot.Add.Crosshair(0, 0);
    }

    public AvaPlot GetPlotControl() => PlotControl;

    public void FinishUpdate()
    {
        Crosshair = PlotControl.Plot.Add.Crosshair(0, 0);
        PlotControl.Refresh();
    }
    private PopupResult TryPopupFromScatter(Coordinates coordinates)
    {
        foreach (var scatter in PlotControl.Plot.GetPlottables<Scatter>())
        {
            var near = scatter.GetNearest(coordinates, PlotControl.Plot.LastRender);
            if (near.IsReal) return new PopupResult(scatter.LegendText, near.X, near.Y, near.Y, false);
        }

        return PopupResult.None;
    }

    private string GetXLabel(double x)
    {
        if (PlotControl.Plot.Axes.Bottom.TickGenerator is FixedDateTimeAutomatic)
            return DateTime.FromOADate(x).ToString();
        return x.ToString();
    }

    private string GetYLabel(double y)
    {
        if (PlotControl.Plot.Axes.Left.TickGenerator is FixedDateTimeAutomatic)
            return DateTime.FromOADate(y).ToString();
        return y.ToString();
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var p = e.GetPosition(PlotControl);
        Pixel mousePixel = new(p.X * PlotControl.DisplayScale, p.Y * PlotControl.DisplayScale);
        var coordinates = PlotControl.Plot.GetCoordinates(mousePixel);


        DebugText.Content = $"X:{GetXLabel(coordinates.X)} Y:{GetYLabel(coordinates.Y)}";
        Crosshair.Position = coordinates;

        var popup = TryPopupFromScatter(coordinates);
        if (!popup.IsValid)
            popup = TryPopupFromBar(coordinates);


        var sb = new StringBuilder();
        sb.AppendLine(popup.Series);
        if (popup.invertAxes)
            sb.AppendLine(GetYLabel(popup.X));
        else
            sb.AppendLine(GetXLabel(popup.X));

        if (popup.Y == popup.V)
            sb.AppendLine(GetYLabel(popup.Y));
        else
            sb.AppendLine((popup.V - popup.Y).ToString());


        myPopupText.Text = sb.ToString();
        //force pop up to move
        if (LastPopup != myPopupText.Text)
            myPopup.IsOpen = false;
        LastPopup = myPopupText.Text;
        myPopup.IsOpen = popup.IsValid;
        PlotControl.Refresh();
    }


    private PopupResult TryPopupFromBar(Coordinates rawcoordinates)
    {
        var inverted = new Coordinates(rawcoordinates.Y, rawcoordinates.X);
        var bestResult = PopupResult.None;
        foreach (var bar in PlotControl.Plot.GetPlottables<BarPlot>())
        {
            var bars = bar.Bars;
            foreach (var b in bars)
            {
                var invert = b.Orientation == Orientation.Horizontal;
                var coordinates = invert
                    ? inverted
                    : rawcoordinates;
                var barLeft = b.Position - b.Size / 2;
                var barright = b.Position + b.Size / 2;

                if (coordinates.X < barLeft)
                    continue;
                if (coordinates.X > barright)
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

    //private void Chart_OnLostFocus(object sender, RoutedEventArgs e) => myPopup.IsOpen = false;

    private readonly record struct PopupResult(string Series, double X, double Y, double V, bool invertAxes)
    {
        internal static readonly PopupResult None = new(string.Empty, 0, 0, 0, false);
        public bool IsValid => Series.IsNotBlank();
    }
}
