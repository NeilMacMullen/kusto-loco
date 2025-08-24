using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using LokqlDx.ViewModels;
using System.Globalization;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace LokqlDx.Views;

public partial class SchemaView : UserControl
{
    public SchemaView()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is TextBlock tb)
        {
            if (tb.DataContext is Schema schema)
            {
                if (sender is not TreeDataGrid tree) return;
                if (tree.DataContext is SchemaViewModel schemaModel)
                    schemaModel.DoubleClickCommand.Execute(new SchemaClick(schema,tb.Text??string.Empty));
            }
        }
    }
}
