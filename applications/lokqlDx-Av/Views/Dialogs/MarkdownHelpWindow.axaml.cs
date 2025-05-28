using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;


namespace LokqlDx.Views.Dialogs;

public partial class MarkdownHelpWindow : UserControl
{
    public MarkdownHelpWindow()
    {
        InitializeComponent();
    }

    private void MarkdownViewer_LayoutUpdated(object? sender, EventArgs e)
    {
        if (MarkdownViewer.Content is Panel panel)
            foreach (var child in panel.Children)
            {
                if (child is TextBlock tb)
                    foreach (var inline in tb.Inlines ?? [])
                        if (inline is InlineUIContainer iuc && iuc.Child is Border br)
                            ColorBorder(br);

                if (child is Grid grid)
                    foreach (var c in grid.Children ?? [])
                        if (c is Border br)
                            ColorBorder(br);

                if (child is Border b) ColorBorder(b);
            }
    }

    private void ColorBorder(Border br)
    {
        var backgroundBrush = this.FindResource("SystemControlBackgroundBaseMediumBrush") as IBrush;
        var borderBrush = this.FindResource("SystemControlTransientBorderBrush") as IBrush;
        br.Background = backgroundBrush;
        br.BorderThickness = new Thickness(1.5);
        br.BorderBrush = borderBrush;
    }
}
