using System.Windows;
using System.Windows.Media;

namespace lokqlDx;

/// <summary>
///     Interaction logic for WorkspacePreferencesWindow.xaml
/// </summary>
public partial class WorkspacePreferencesWindow : Window
{
    public  Workspace _workspace;
    private readonly UIPreferences _uiPreferences;
   

    public WorkspacePreferencesWindow(Workspace workspace,UIPreferences uiPreferences)
    {
        _workspace = workspace;
        _uiPreferences = uiPreferences;

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
        StartupScript.FontSize = _uiPreferences.FontSize;
        StartupScript.FontFamily = new FontFamily(_uiPreferences.FontFamily);
    }
}
