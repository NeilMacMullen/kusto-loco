using System.Windows;
using Microsoft.Web.WebView2.Core;
using NotNullStrings;

namespace lokqlDx;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        EnsureWebViewAvailable();
        Window window = new MainWindow(e.Args);
        window.Show();
    }

    private void EnsureWebViewAvailable()
    {
        try
        {
            var webViewVersion = CoreWebView2Environment.GetAvailableBrowserVersionString();
            if (webViewVersion.IsNotBlank())
                return;
        }
        catch
        {
            // ignored
        }

        var dlg = new MissingWebviewWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        dlg.ShowDialog();
        Application.Current.Shutdown();

    }
}
