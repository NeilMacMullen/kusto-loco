using System.Windows;
using System.Windows.Controls;
using KustoLoco.Core;
using KustoLoco.Core.Settings;

namespace lokqlDx;

/// <summary>
///     Interaction logic for FlyoutResult.xaml
/// </summary>
public partial class FlyoutResult : Window
{
    private readonly KustoQueryResult _result;
    private readonly KustoSettingsProvider _settings;
    private readonly WpfRenderingSurface _wpfRenderingSurface;


    public FlyoutResult(KustoQueryResult result, KustoSettingsProvider settings)
    {
        _result = result;
        _settings = settings.Snapshot();

        InitializeComponent();
        Title = "Result";
        _wpfRenderingSurface = new WpfRenderingSurface(RenderingSurface, dataGrid,
            DatagridOverflowWarning, WpfPlot1,
            settings);
    }

    private async void WpfPlot1_OnLoaded(object sender, RoutedEventArgs e) =>
        await _wpfRenderingSurface.RenderToDisplay(_result);

    private void OnCopyImageToClipboard(object sender, RoutedEventArgs e) => _wpfRenderingSurface.CopyToClipboard();


    private void OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.PropertyType == typeof(DateTime))
            if (e.Column is DataGridTextColumn textColumn)
            {
                var fmt = _settings.GetOr("datagrid.datetime_format", "dd MMM yyyy HH:mm");
                textColumn.Binding.StringFormat = fmt;
            }
    }

    private void OnRename(object sender, RoutedEventArgs e)
    {
        var dlg = new EnterTitle(Title);
        if (dlg.ShowDialog() == true)
            Title = dlg.Text;
    }
}
