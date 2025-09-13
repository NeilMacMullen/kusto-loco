using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using lokqlDx;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace LokqlDx.ViewModels;

public partial class ChartViewModel : ObservableObject
{
    private KustoQueryResult Result = KustoQueryResult.Empty;
    public ChartViewModel(KustoSettingsProvider kustoSettings)
    {
        _kustoSettings = kustoSettings;
    }
    private IScottPlotHost _plotter = new NullScottPlotHost();
    [RelayCommand]
    public void ToggleStyling()
    {
        // Implementation for toggling styles
    }

    [ObservableProperty] private bool _show;
    private readonly KustoSettingsProvider _kustoSettings;
    public void Activate(bool onOff) => Show = onOff;

    public void Render(KustoQueryResult result)
    {
        _plotter.RenderToDisplay(result, _kustoSettings);
        Result = result;
    }
    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
        => _plotter.RenderToImage(result, pWidth, pHeight, _kustoSettings);

    public void CopyToClipboard()
    {
        _plotter.CopyToClipboard();
    }
    public void RegisterHost(IScottPlotHost plotter)
    {
        //we have to rerender every time the view's data context is changed
        //because in tabbed views, the same Chartview may be
        //reused with multiple datacontexts and we don't have a way of exposing
        //the chart as an observable property
        if (plotter != _plotter)
        {
            _plotter = plotter;
            _plotter.RenderToDisplay(Result, _kustoSettings);
        }
    }

}
