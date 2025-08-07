using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using LokqlDx.ViewModels.Dialogs;

namespace LokqlDx.ViewModels;

public partial class FlyoutViewModel : ObservableObject, IDialogViewModel
{
    private readonly KustoQueryResult _result;
    [ObservableProperty] private RenderingSurfaceViewModel _renderingSurface;

    public FlyoutViewModel(KustoQueryResult result, KustoSettingsProvider settings,
        DisplayPreferencesViewModel displayPreferences)
    {
        _result = result;
        var settings1 = settings.Snapshot();
        _renderingSurface = new RenderingSurfaceViewModel("flyout", settings1, displayPreferences,new NullConsole());
    }

    public Task Result { get; } = Task.CompletedTask;

    public async Task InitialRender() => await RenderingSurface.RenderToDisplay(_result);

    [RelayCommand]
    public async Task Refresh() => await InitialRender();
}
