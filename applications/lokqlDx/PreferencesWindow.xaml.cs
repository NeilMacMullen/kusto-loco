using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace lokqlDx
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private readonly Preferences _preferences;

        public PreferencesWindow(Preferences preferences)
        {
            _preferences = preferences;
            InitializeComponent();
        }

        private void PreferencesWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
           StartupScript.Text= _preferences.StartupScript;
           var fontFamilies = Fonts.SystemFontFamilies;
           FontSelector.ItemsSource =fontFamilies ;
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
}
