using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LokqlDx.ViewModels;
using Mapsui.UI.Avalonia;
using NotNullStrings;

namespace LokqlDx.Views;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
    }

 
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        RegisterIfPossible();
        base.OnSizeChanged(e);
    }

    /// <summary>
    ///     Bind to the ViewModel
    /// </summary>
    /// <remarks>
    ///     Because Avalonia uses view sharing we run a risk of
    ///     registering the wrong control to the view model initially
    ///     The most reliable way around this seems to be to catch the moment
    ///     when the control resizes from 0,0 and to register then
    /// </remarks>
    private void RegisterIfPossible()
    {
        if (DataContext is MapViewModel vm &&
            Bounds.Size is { Width: > 0, Height: > 0 })
            vm.OnCopyToClipboard = CopyMapToClipboard;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        RegisterIfPossible();
        base.OnDataContextChanged(e);
    }

    private void CopyMapToClipboard()
    {
        ControlRenderer.SaveControlToClipboard(this);
    }

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not MapControl map)
            return;
        if (map.DataContext is not MapViewModel vm) return;

        var screen = e.GetPosition(map);

        var label = vm.GetTooltipAtPosition(screen);
        if (label.IsBlank())
            ToolTip.SetTip(map, null);
        else
            ToolTip.SetTip(map, label);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void TheMap_OnLoaded(object? sender, RoutedEventArgs e)
    {
        RegisterIfPossible();
    }
}
