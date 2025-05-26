using System.Windows;
using System.Windows.Media;

namespace lokqlDx;

/// <summary>
///     Interaction logic for ApplicationPreferencesWindow.xaml
/// </summary>
public partial class ApplicationPreferencesWindow : Window
{
    private readonly ApplicationPreferences _preferences;
    private readonly UIPreferences _uiPreferences;


    public ApplicationPreferencesWindow(ApplicationPreferences preferences,UIPreferences uiPreferences)
    {
        _preferences = preferences;
        _uiPreferences = uiPreferences;

        InitializeComponent();
    }

    private void Dialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        StartupScript.Text = _preferences.StartupScript;
        StartupScript.FontSize = _uiPreferences.FontSize;
        StartupScript.FontFamily = new FontFamily(_uiPreferences.FontFamily);
        var fontFamilies = Fonts.SystemFontFamilies;
        FontSelector.ItemsSource = fontFamilies;
        Autosave.IsChecked = _preferences.AutoSave;
        var current = fontFamilies.Where(f => f.Source == _uiPreferences.FontFamily).ToArray();
        if (current.Any())
            FontSelector.SelectedItem = current.First();
    }
    
    private void OnOk(object sender, RoutedEventArgs e)
    {
        _preferences.StartupScript = StartupScript.Text;
        _uiPreferences.FontFamily = ((FontFamily)FontSelector.SelectedItem).Source;
        _preferences.AutoSave = Autosave.IsChecked==true;
        DialogResult = true;
    }
}
