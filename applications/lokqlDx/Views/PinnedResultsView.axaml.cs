using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

public partial class PinnedResultsView : UserControl
{
    public PinnedResultsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);


    private void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // Find the parent container (e.g., DockPanel or Grid)
            var container = button.GetVisualAncestors().OfType<DockPanel>().FirstOrDefault();
            if (container != null)
            {
                // Find the TextBox named "EditTextBox" in the same container
                var textBox = container.GetVisualDescendants().OfType<TextBox>()
                    .FirstOrDefault(tb => tb.Name == "EditTextBox");
                if (textBox is not null)
                    Dispatcher.UIThread.Post(() =>
                    {
                        textBox.Focus();
                        textBox.CaretIndex = textBox.Text?.Length ?? 0;
                        // Optionally select all text:
                        // textBox.SelectionStart = 0;
                        // textBox.SelectionEnd = textBox.Text?.Length ?? 0;
                    }, DispatcherPriority.Background);
            }
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        if (DataContext is PinnedResultsViewModel vm && sender is TextBox tb && tb.DataContext is PinnedKustoResult res)
            vm.FilterEnterCommand.Execute(res);
    }
}
