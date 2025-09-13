using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Clowd.Clipboard;
using DocumentFormat.OpenXml.Spreadsheet;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.ScottPlot;
using lokqlDx;
using LokqlDx.ViewModels;
using ScottPlot;
using ScottPlot.AxisRules;
using ScottPlot.Plottables;

namespace LokqlDx.Views;

public partial class ChartView : UserControl, IScottPlotHost
{
    private  Crosshair _crosshair;

    private string _lastPopup = string.Empty;
    private bool _cursorEnabled = false;
    public ChartView()
    {
        InitializeComponent();
        _crosshair = PlotControl.Plot.Add.Crosshair(0, 0);
        _crosshair.IsVisible = _cursorEnabled;
        Messaging.RegisterForEvent<ToggleCursorMessage>(this,ToggleCursor);
    }

    private void ToggleCursor()
    {
        _cursorEnabled = !_cursorEnabled;
        ResetCursor();
    }

    private void ResetCursor()
    {
        _crosshair.IsVisible = _cursorEnabled;
        myPopup.IsOpen = _cursorEnabled;
    }

    private PopupSupport PopupSupport => new(PlotControl.Plot);


    #region cursor/popup

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_cursorEnabled)
        {
            myPopup.IsOpen = false;
            return;
        }

        var p = e.GetPosition(PlotControl);
        Pixel mousePixel = new(p.X * PlotControl.DisplayScale, p.Y * PlotControl.DisplayScale);
        var coordinates = PlotControl.Plot.GetCoordinates(mousePixel);


        DebugText.Content = PopupSupport.GetBasicPositionInfo(coordinates);
        _crosshair!.Position = coordinates;

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
        if (OperatingSystem.IsWindows())
            try
            {
                var width = (int)PlotControl.Bounds.Width;
                var height = (int)PlotControl.Bounds.Height;
                var bytes = PlotControl.Plot.GetImageBytes(width, height, ImageFormat.Png);

                using var memoryStream = new MemoryStream(bytes);
                var bitmap = new Bitmap(memoryStream);
                ClipboardAvalonia.SetImage(bitmap);
            }
            catch
            {
            }
    }

    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight,
        KustoSettingsProvider kustoSettings)
        => ScottPlotKustoResultRenderer.RenderToImage(result, ImageFormat.Png, (int)pWidth, (int)pHeight,
            kustoSettings);

    public void RenderToDisplay(KustoQueryResult result, KustoSettingsProvider kustoSettings) =>
        //important - this can be called from inside the engine so we need to dispatch it to the UI thread
        DispatcherHelper.SafeInvoke(() =>
        {
            PlotControl.Reset();
            var plot = PlotControl.Plot;
            plot.Clear();
            var renderInfo =ScottPlotKustoResultRenderer.RenderToPlot(plot, result, kustoSettings);
            _crosshair = plot.Add.Crosshair(0, 0);
            ResetCursor();
            plot.Axes.Rules.Clear();
            if (renderInfo.HasYSpan)
            {
                renderInfo = renderInfo.InflateY(0.01);
                plot.Axes.Rules.Add(new LockedVertical(plot.Axes.Left, renderInfo.MinY, renderInfo.MaxY));
            }

            PlotControl.Refresh();
        });

    #endregion

    #region registration


    private void RegisterHost(ChartView chart)
    {
        if (chart.DataContext is ChartViewModel vm)
        {
            var newState = new RegistrationState(vm, chart);
            // if (_registrationState == newState)
            //return;

            vm.RegisterHost(chart);
            _registrationState = newState;
        }
    }
    private void ChartView_OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is ChartView chart)
            RegisterHost(chart);
    }

    private void ChartView_OnDataContextChanged_OnDataContextChanged(object? sender, EventArgs e)
    {
        if (sender is ChartView chart)
            RegisterHost(chart);
    }
    private RegistrationState _registrationState = new(null, null);
    private record struct RegistrationState(ChartViewModel? ViewModel, ChartView? ChartView);

    #endregion


}
