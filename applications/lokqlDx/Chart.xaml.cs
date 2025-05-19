using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using KustoLoco.Rendering.ScottPlot;
using NotNullStrings;
using ScottPlot;
using ScottPlot.Plottables;
using Coordinates = ScottPlot.Coordinates;
using Orientation = ScottPlot.Orientation;

namespace lokqlDx;

public interface IScottPlotHost
{
    Plot GetPlot(bool reset);
    void FinishUpdate();
    void CopyToClipboard();
}

/// <summary>
///     Interaction logic for Chart.xaml
/// </summary>
public partial class Chart : UserControl, IScottPlotHost
{
    private Crosshair Crosshair;

    private string LastPopup = string.Empty;

    public Chart()
    {
        InitializeComponent();
        Crosshair = WpfPlot1.Plot.Add.Crosshair(0, 0);

        MouseMove += DisplayScaling_MouseMove;
    }

    public Plot GetPlot(bool reset)
    {
        if (reset)
            WpfPlot1.Reset();
        return WpfPlot1.Plot;
    }

    public void FinishUpdate()
    {
        Crosshair = WpfPlot1.Plot.Add.Crosshair(0, 0);
        WpfPlot1.Refresh();
    }

    public void CopyToClipboard()
    {
        try
        {
            var bytes = WpfPlot1.Plot.GetImageBytes((int)WpfPlot1.ActualWidth,
                (int)WpfPlot1.ActualHeight, ImageFormat.Png);
            using var memoryStream = new MemoryStream(bytes);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freeze the image to make it cross-thread accessible
            Clipboard.SetImage(bitmapImage);
        }
        catch
        {
        }
    }

    private PopupResult TryPopupFromScatter(Coordinates coordinates)
    {
        foreach (var scatter in WpfPlot1.Plot.GetPlottables<Scatter>())
        {
            var near = scatter.GetNearest(coordinates, WpfPlot1.Plot.LastRender);
            if (near.IsReal) return new PopupResult(scatter.LegendText, near.X, near.Y, near.Y, false);
        }

        return PopupResult.None;
    }

    private string GetXLabel(double x)
    {
        if (WpfPlot1.Plot.Axes.Bottom.TickGenerator is FixedDateTimeAutomatic)
            return DateTime.FromOADate(x).ToString();
        return x.ToString();
    }

    private string GetYLabel(double y)
    {
        if (WpfPlot1.Plot.Axes.Left.TickGenerator is FixedDateTimeAutomatic)
            return DateTime.FromOADate(y).ToString();
        return y.ToString();
    }


    private void DisplayScaling_MouseMove(object sender, MouseEventArgs e)
    {
        var p = e.GetPosition(WpfPlot1);
        Pixel mousePixel = new(p.X * WpfPlot1.DisplayScale, p.Y * WpfPlot1.DisplayScale);
        var coordinates = WpfPlot1.Plot.GetCoordinates(mousePixel);


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
        WpfPlot1.Refresh();
    }


    private PopupResult TryPopupFromBar(Coordinates rawcoordinates)
    {
        var inverted = new Coordinates(rawcoordinates.Y, rawcoordinates.X);
        var bestResult = PopupResult.None;
        foreach (var bar in WpfPlot1.Plot.GetPlottables<BarPlot>())
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

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => Crosshair = WpfPlot1.Plot.Add.Crosshair(0, 0);

    private void Chart_OnLostFocus(object sender, RoutedEventArgs e) => myPopup.IsOpen = false;

    private readonly record struct PopupResult(string Series, double X, double Y, double V, bool invertAxes)
    {
        internal static readonly PopupResult None = new(string.Empty, 0, 0, 0, false);
        public bool IsValid => Series.IsNotBlank();
    }
}
