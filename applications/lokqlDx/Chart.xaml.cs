using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KustoLoco.Rendering.ScottPlot;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace lokqlDx;

/// <summary>
///     Interaction logic for Chart.xaml
/// </summary>
public partial class Chart : UserControl
{
    private Crosshair Crosshair;

    private string LastPopup = string.Empty;

    public Chart()
    {
        InitializeComponent();
        Crosshair = WpfPlot1.Plot.Add.Crosshair(0, 0);

        MouseMove += DisplayScaling_MouseMove;
    }

    public WpfPlot Plot => WpfPlot1;

    private void DisplayScaling_MouseMove(object sender, MouseEventArgs e)
    {
        var p = e.GetPosition(WpfPlot1);
        Pixel mousePixel = new(p.X * WpfPlot1.DisplayScale, p.Y * WpfPlot1.DisplayScale);
        var coordinates = WpfPlot1.Plot.GetCoordinates(mousePixel);
        DebugText.Content = $"Mouse: {coordinates.X}  {coordinates.Y}";
        Crosshair.Position = coordinates;
        var isPopupOpen = false;
        var sb = new StringBuilder();

        foreach (var scatter in WpfPlot1.Plot.GetPlottables<Scatter>())
        {
            var text = scatter.LegendText;
          
            var near = scatter.GetNearest(coordinates, WpfPlot1.Plot.LastRender);

            if (near.IsReal)
            {
                var x = near.X.ToString();
                var allTicks = WpfPlot1.Plot.Axes.Bottom.TickGenerator.Ticks;
                if (WpfPlot1.Plot.Axes.Bottom.TickGenerator is FixedDateTimeAutomatic dt)
                    x = DateTime.FromOADate(near.X).ToString();
                sb.AppendLine($"{text} {x}, {near.Y}");
                isPopupOpen = true;
                break;
            }
        }

        var hitBar = new Bar
        {
            Position = double.MaxValue, ValueBase = double.MaxValue,
            Value = double.MaxValue
        };
        var barText = string.Empty;
        foreach (var bar in WpfPlot1.Plot.GetPlottables<BarPlot>())
        {
            var bars = bar.Bars;
            foreach (var b in bars)
            {
                if (b.Position<coordinates.X)
                    continue;
                if (b.ValueBase >coordinates.Y)
                    continue;
                if (b.Value< coordinates.Y)
                    continue;
                //we have a candidate
                if (b.Position <= hitBar.Position)
                {
                    hitBar = b;
                    barText = bar.LegendText;
                }
            }
        }

        if (hitBar.Position < double.MaxValue)
        {
            sb.AppendLine($"{barText} {hitBar.Position} {hitBar.Value}");
            isPopupOpen = true;
        }

        myPopupText.Text = sb.ToString();
        //force pop up to move
        if (LastPopup != myPopupText.Text)
            myPopup.IsOpen = false;
        LastPopup = myPopupText.Text;
        myPopup.IsOpen = isPopupOpen;
        WpfPlot1.Refresh();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => Crosshair = WpfPlot1.Plot.Add.Crosshair(0, 0);
}
