using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Clowd.Clipboard;
using LokqlDx.ViewModels;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.UI.Avalonia;
using NotNullStrings;

namespace LokqlDx.Views;

public partial class MapView : UserControl
{
    private Point _lastPointerPosition;

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

    private void CopyMapToClipboard() => ControlRenderer.SaveControlToClipboard(this);

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not MapControl map)
            return;
        if (map.DataContext is not MapViewModel vm) return;

        var screen = e.GetPosition(map);
        _lastPointerPosition = screen;

        var label = vm.GetTooltipAtPosition(screen);
        ToolTip.SetTip(map, label.IsBlank() ? null : label);
    }

    private (double lon, double lat)? GetLatLongAtLastPointerPosition()
    {
        if (DataContext is not MapViewModel vm)
            return null;

        var vp = vm.Map.Navigator.Viewport;
        var world = vp.ScreenToWorld(_lastPointerPosition.X, _lastPointerPosition.Y);
        return SphericalMercator.ToLonLat(world.X, world.Y);
    }

    private void OpenInGoogleMaps_Click(object? sender, RoutedEventArgs e)
    {
        var lonLat = GetLatLongAtLastPointerPosition();
        if (lonLat is null)
            return;

        var url = $"https://www.google.com/maps?q={lonLat.Value.lat:F6},{lonLat.Value.lon:F6}";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private void CopyLatLongToClipboard_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var lonLat = GetLatLongAtLastPointerPosition();
            if (lonLat is null)
                return;

            var text = $"{lonLat.Value.lat:F6}, {lonLat.Value.lon:F6}";
            if (OperatingSystem.IsWindows())
                ClipboardAvalonia.SetText(text);
        }
        catch
        {
            // Silently ignore clipboard errors
        }
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void TheMap_OnLoaded(object? sender, RoutedEventArgs e) => RegisterIfPossible();
}
