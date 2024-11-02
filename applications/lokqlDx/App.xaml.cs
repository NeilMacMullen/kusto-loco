using System.Windows;

namespace lokqlDx;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Window window = new MainWindow(e.Args);
        window.Show();
    }
}
