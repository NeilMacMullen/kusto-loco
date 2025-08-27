using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;

namespace LokqlDx.Views.Dialogs;

public partial class ApplicationPreferencesView : UserControl
{
    public ApplicationPreferencesView()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, ThemeChanged);
    }

    private void ThemeChanged(object recipient, ThemeChangedMessage message) =>
        HighlightHelper.ApplySyntaxHighlighting(TextEditor);

    private void UserControl_Loaded(object? sender, RoutedEventArgs e) =>
        HighlightHelper.ApplySyntaxHighlighting(TextEditor);
}
