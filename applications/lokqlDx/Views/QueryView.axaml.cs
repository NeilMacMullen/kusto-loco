using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace LokqlDx.Views;

public partial class QueryView : UserControl
{
    public QueryView()
    {
        InitializeComponent();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
    }

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        TheQueryEditor.Focus();
    }
}
