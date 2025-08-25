using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using SkiaSharp.HarfBuzz;
using System.ComponentModel;
using KustoLoco.Core.Settings;
using static KustoLoco.Core.KustoFormatter;

namespace LokqlDx.ViewModels;

public partial class ResultDisplayViewModel : Document, INotifyPropertyChanged
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private KustoQueryResult _result;
    [ObservableProperty] private RenderingSurfaceViewModel _renderingSurface;
    public ResultDisplayViewModel(string name,KustoQueryResult result)
    {
        Title = name;
        Result = result;
        RenderingSurface =
            new RenderingSurfaceViewModel("name", new KustoSettingsProvider(),
                new DisplayPreferencesViewModel(), new NullConsole());


    }
}
