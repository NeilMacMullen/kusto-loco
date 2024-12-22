using System.Windows;

namespace lokqlDx;

/// <summary>
///     Interaction logic for WorkspaceOptions.xaml
/// </summary>
public partial class WorkspaceOptions : Window
{
    public  Workspace _workspace;

    public WorkspaceOptions(Workspace workspace)
    {
        _workspace = workspace;
        InitializeComponent();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        _workspace = _workspace with {StartupScript = StartupScript.Text};
        DialogResult = true;
    }

    private void WorkspaceOptions_OnLoaded(object sender, RoutedEventArgs e)
    {
        StartupScript.Text = _workspace.StartupScript;
    }
}
