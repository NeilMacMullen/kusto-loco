using System.Windows;

namespace lokqlDx;

/// <summary>
///     Interaction logic for EnterTitle.xaml
/// </summary>
public partial class EnterTitle : Window
{
    public EnterTitle(string text)
    {
        InitializeComponent();
        TextBox.Text = text;
    }

    public string Text { get; set; } = string.Empty;

    private void OnOk(object sender, RoutedEventArgs e)
    {
        Text = TextBox.Text;
        DialogResult = true;
    }
}
