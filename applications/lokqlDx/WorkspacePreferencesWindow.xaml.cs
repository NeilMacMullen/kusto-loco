using System.Windows;

namespace lokqlDx;

/// <summary>
///     Interaction logic for WorkspacePreferencesWindow.xaml
/// </summary>
public partial class WorkspacePreferencesWindow : Window
{
    public  Workspace _workspace;

    public WorkspacePreferencesWindow(Workspace workspace)
    {
        _workspace = workspace;
        InitializeComponent();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        _workspace = _workspace with {StartupScript = StartupScript.Text};
        DialogResult = true;
    }

    private void Dialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        StartupScript.Text = _workspace.StartupScript;
    }
}
