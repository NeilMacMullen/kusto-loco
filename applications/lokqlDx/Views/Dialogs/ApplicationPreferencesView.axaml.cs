using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LokqlDx.Views.Dialogs;

public partial class ApplicationPreferencesView : UserControl
{
    public ApplicationPreferencesView()
    {
        InitializeComponent();
        Messaging.RegisterForEvent<ThemeChangedMessage>(this, OnThemeChanged);
    }

    private void OnThemeChanged()
        => HighlightHelper.ApplySyntaxHighlighting(TextEditor);


    private void UserControl_Loaded(object? sender, RoutedEventArgs e) =>
        HighlightHelper.ApplySyntaxHighlighting(TextEditor);
}
