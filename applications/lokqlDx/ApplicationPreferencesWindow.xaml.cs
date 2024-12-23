using System.Windows;
using System.Windows.Media;

namespace lokqlDx;

/// <summary>
///     Interaction logic for ApplicationPreferencesWindow.xaml
/// </summary>
public partial class ApplicationPreferencesWindow : Window
{
    private readonly Preferences _preferences;

    public ApplicationPreferencesWindow(Preferences preferences)
    {
        _preferences = preferences;
        InitializeComponent();
    }

    private void Dialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        StartupScript.Text = _preferences.StartupScript;
        var fontFamilies = Fonts.SystemFontFamilies;
        FontSelector.ItemsSource = fontFamilies;
        var current = fontFamilies.Where(f => f.Source == _preferences.FontFamily).ToArray();
        if (current.Any())
            FontSelector.SelectedItem = current.First();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        _preferences.StartupScript = StartupScript.Text;
        _preferences.FontFamily = ((FontFamily)FontSelector.SelectedItem).Source;
        DialogResult = true;
    }
}
