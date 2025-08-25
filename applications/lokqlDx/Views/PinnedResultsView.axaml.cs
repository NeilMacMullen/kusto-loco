using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using LokqlDx.ViewModels;
using System.Globalization;

namespace LokqlDx.Views;

public partial class PinnedResultsView : UserControl
{
    public PinnedResultsView()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

  
    private void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(sender is Button button)
        {
            // Find the parent container (e.g., DockPanel or Grid)
            var container = button.GetVisualAncestors().OfType<DockPanel>().FirstOrDefault();
            if (container != null)
            {
                // Find the TextBox named "EditTextBox" in the same container
                var textBox = container.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(tb => tb.Name == "EditTextBox");
                if (textBox is not null)
                {
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
    }
}
