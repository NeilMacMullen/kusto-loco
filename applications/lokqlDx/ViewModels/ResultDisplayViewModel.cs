using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;

namespace LokqlDx.ViewModels;

public partial class ResultDisplayViewModel : Document, INotifyPropertyChanged
{
    [ObservableProperty] private RenderingSurfaceViewModel _renderingSurface;
    [ObservableProperty] private KustoQueryResult _result;

    public ResultDisplayViewModel(string name, KustoQueryResult result)
    {
        Title = name;
        Result = result;
        RenderingSurface =
            new RenderingSurfaceViewModel("name", new KustoSettingsProvider(),
                new DisplayPreferencesViewModel(), new NullConsole())
            {
                Result = Result
            };
    }
}
