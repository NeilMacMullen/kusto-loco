using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit;

namespace LokqlDx.Views.Dialogs;

public partial class KqlFormattingPreferencesView : UserControl
{
    public KqlFormattingPreferencesView()
    {
        InitializeComponent();
        Messaging.RegisterForEvent<ThemeChangedMessage>(this, OnThemeChanged);
    }

    private void OnThemeChanged()
    {
        var textEditor = this.FindControl<TextEditor>("TextEditor");
        if (textEditor != null)
            HighlightHelper.ApplySyntaxHighlighting(textEditor);
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        var textEditor = this.FindControl<TextEditor>("TextEditor");
        if (textEditor != null)
            HighlightHelper.ApplySyntaxHighlighting(textEditor);
    }
}
