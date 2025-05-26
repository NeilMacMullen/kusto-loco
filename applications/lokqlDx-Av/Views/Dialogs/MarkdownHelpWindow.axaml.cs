using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

#pragma warning disable VSTHRD100

namespace LokqlDx.Views.Dialogs;

public partial class MarkdownHelpWindow : UserControl
{
    public MarkdownHelpWindow()
    {
        InitializeComponent();
    }

    private void MarkdownViewer_LayoutUpdated(object? sender, System.EventArgs e)
    {
        
        if (MarkdownViewer.Content is Panel panel)
        {
            foreach(var child in panel.Children)
            {
                if (child is TextBlock tb)
                {
                    foreach( var inline in tb.Inlines ?? [])
                    {
                        if (inline is InlineUIContainer iuc && iuc.Child is Border br)
                        {
                            ColorBorder(br);
                        }
                    }
                }
                if (child is Grid grid)
                {
                    foreach (var c in grid.Children ?? [])
                    {
                        if (c is Border br)
                        {
                            ColorBorder(br);
                        }
                    }
                }
                if (child is Border b)
                {
                    ColorBorder(b);
                }
            }
        }
    }

    private void ColorBorder(Border br)
    {
        var backgroundBrush = this.FindResource("SystemControlBackgroundBaseMediumBrush") as IBrush;
        var borderBrush = this.FindResource("SystemControlTransientBorderBrush") as IBrush;
        br.Background = backgroundBrush;
        br.BorderThickness = new Avalonia.Thickness(1.5);
        br.BorderBrush = borderBrush;
    }
}

/*
/// <summary>
///     Interaction logic for MarkdownHelpWindow.xaml
/// </summary>
public partial class MarkdownHelpWindow : Window
{
    private const string BrowserPrefix = @"https://github.com/NeilMacMullen/kusto-loco/wiki";
    private const string RawPrefix = @"https://raw.githubusercontent.com/wiki/NeilMacMullen/kusto-loco";
    private readonly string _pageName;

    public MarkdownHelpWindow(string pageName)
    {
        _pageName = pageName;
        InitializeComponent();
        Owner = Application.Current.MainWindow;
        Title= pageName;
    }

    private string MakeUri(string prefix)
    {
        var escaped = Uri.EscapeDataString(_pageName);
        return $"{prefix}/{escaped}";
    }


    private async void MarkdownHelpWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var rawLink = MakeUri(RawPrefix) + ".md";
            using var client = new HttpClient();
            var text = await client.GetStringAsync(rawLink);
            Viewer.Markdown = text;
        }
        catch
        {
            Viewer.Markdown = "Error loading page";
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var link = MakeUri(BrowserPrefix);
        OpenUriInBrowser(link);
    }

    private static void OpenUriInBrowser(string uri) =>
        Process.Start(new ProcessStartInfo { FileName = uri, UseShellExecute = true });
}
*/
