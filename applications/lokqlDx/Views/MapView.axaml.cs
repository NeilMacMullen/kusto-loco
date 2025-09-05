using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LokqlDx.ViewModels;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using NotNullStrings;

namespace LokqlDx.Views;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
        TheMap.LayoutUpdated += (_, _) =>
        {
            if (TheMap.IsEffectivelyVisible)
            {
                //  ForceUpdate();
            }
        };


        TheMap.PointerMoved += OnMapPointerMoved;
    }

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        var vm = DataContext as MapViewModel;
        if (vm is null || TheMap.Map is null) return;

        var screen = e.GetPosition(TheMap);

        var label = vm.GetTooltipAtPosition(screen);
        if (label.IsBlank())
            ToolTip.SetTip(TheMap, label);
        else 
            ToolTip.SetTip(TheMap, label);
    }

   


    private void ForceUpdate()
{
    var extent = TheMap.Map.Extent;

    TheMap.Map.Navigator.ZoomToBox(extent);
    TheMap.RefreshGraphics();
}

private void InitializeComponent()
{
    AvaloniaXamlLoader.Load(this);
}
}
