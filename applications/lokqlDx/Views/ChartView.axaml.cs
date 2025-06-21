using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Clowd.Clipboard;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.ScottPlot;
using lokqlDx;
using ScottPlot;
using ScottPlot.Plottables;

namespace LokqlDx.Views;

public partial class ChartView : UserControl, IScottPlotHost
{
    private Crosshair _crosshair;

    private string _lastPopup = string.Empty;

    public ChartView()
    {
        InitializeComponent();
        _crosshair = PlotControl.Plot.Add.Crosshair(0, 0);
    }

    private PopupSupport PopupSupport => new(PlotControl.Plot);


    #region cursor/popup

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var p = e.GetPosition(PlotControl);
        Pixel mousePixel = new(p.X * PlotControl.DisplayScale, p.Y * PlotControl.DisplayScale);
        var coordinates = PlotControl.Plot.GetCoordinates(mousePixel);


        DebugText.Content = PopupSupport.GetBasicPositionInfo(coordinates);
        _crosshair.Position = coordinates;

        var popup = PopupSupport.TryPopup(coordinates);

        myPopupText.Text = PopupSupport.GetPopupContents(popup);
        //force pop up to move
        if (_lastPopup != myPopupText.Text)
            myPopup.IsOpen = false;
        _lastPopup = myPopupText.Text;
        myPopup.IsOpen = popup.IsValid;
        PlotControl.Refresh();
    }

    #endregion


    #region IScottplotHost

    public void CopyToClipboard()
    {
        try
        {
            var bytes = PlotControl.Plot.GetImageBytes((int)PlotControl.Width,
                (int)PlotControl.Height, ImageFormat.Png);
            using var memoryStream = new MemoryStream(bytes)
            {
                //TODO - not sure how to implement avalonia clipboard support
            };
        }
        catch
        {
        }
    }

    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight,
        KustoSettingsProvider kustoSettings)
        => ScottPlotKustoResultRenderer.RenderToImage(result,ImageFormat.Png,(int)pWidth, (int)pHeight,kustoSettings);

    public void RenderToDisplay(KustoQueryResult result, KustoSettingsProvider kustoSettings) =>
        //important - this can be called from inside the engine so we need to dispatch it to the UI thread
        DispatcherHelper.SafeInvoke(() =>
        {
            PlotControl.Reset();
            var plot = PlotControl.Plot;
            plot.Clear();
            ScottPlotKustoResultRenderer.RenderToPlot(plot, result, kustoSettings);
            _crosshair = PlotControl.Plot.Add.Crosshair(0, 0);
            PlotControl.Refresh();
        });

    #endregion

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                var width = (int)PlotControl.Bounds.Width;
                var height = (int)PlotControl.Bounds.Height;
                var bytes = PlotControl.Plot.GetImageBytes(width, height, ImageFormat.Png);

                using var memoryStream = new MemoryStream(bytes);
                var bitmap = new Bitmap(memoryStream);
                ClipboardAvalonia.SetImage(bitmap);
            }
            catch { }
        }

    }
}
