using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace lokqlDx;

/// <summary>
///     Interaction logic for MissingWebviewWindow.xaml
/// </summary>
public partial class MissingWebviewWindow : Window
{
    public MissingWebviewWindow()
    {
        InitializeComponent();
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        var url = e.Uri.AbsoluteUri;
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        e.Handled = true;
    }
}
