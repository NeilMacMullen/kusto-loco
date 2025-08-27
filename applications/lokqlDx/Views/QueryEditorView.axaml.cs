using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;
using LokqlDx.ViewModels;
using lokqlDxComponents;
using lokqlDxComponents.Views.Dialogs;

#pragma warning disable VSTHRD100

namespace LokqlDx.Views;

public partial class QueryEditorView : UserControl
{
    private readonly EditorHelper _editorHelper;
    private CompletionManager? _completionManager;


    public QueryEditorView()
    {
        InitializeComponent();
        _editorHelper = new EditorHelper(TextEditor);
        Messaging.RegisterForEvent<ThemeChangedMessage>(this, OnThemeChanged);
    }

    private void OnThemeChanged()
    {
        HighlightHelper.ApplySyntaxHighlighting(TextEditor);
    }

    private QueryEditorViewModel? GetVm() =>
        (DataContext as QueryEditorViewModel)!;


    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        HighlightHelper.ApplySyntaxHighlighting(TextEditor);
        //Ensure we can intercept ENTER key
        TextEditor.AddHandler(KeyDownEvent, InternalEditor_OnKeyDown, RoutingStrategies.Tunnel);
        TextEditor.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        TextEditor.AddHandler(DragDrop.DragOverEvent, DragOver);
        TextEditor.AddHandler(DragDrop.DropEvent, Drop);
        TextEditor.TextArea.TextEntered += TextArea_TextEntered;
        TextEditor.TextArea.TextView.LinkTextUnderline = true;
        TextEditor.TextArea.TextView.LinkTextForegroundBrush = Brushes.LightGreen;
    }

    private async void TextArea_TextEntered(object? sender, TextInputEventArgs e)
    {
        if (_completionManager is not null) await _completionManager.HandleKeyDown(e);
    }

    private void DragOver(object? sender, DragEventArgs drgevent)
        => DragDropManager.DragOver(drgevent);

    private void DragEnter(object? sender, DragEventArgs drgevent)
        => DragDropManager.DragEnter(drgevent);

    private void Drop(object? sender, DragEventArgs e)
        => DragDropManager.Drop(e, _editorHelper);


    private async void InternalEditor_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (GetVm() is not { } vm)
            return;
        await QueryExecutionHelper.HandleKeyCombo(e, _editorHelper, vm);
        vm.EditorOffset = _editorHelper.CurrentOffset();
    }


    private void TextEditor_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (GetVm() is not { } vm)
            return;
        _completionManager = new CompletionManager(TextEditor, _editorHelper, vm,
            new CompletionWindowWrapper(TextEditor.TextArea));
        TextEditor.TextArea.Focus();
    }

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e) => TextEditor.Focus();
}
