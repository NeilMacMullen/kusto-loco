using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using LokqlDx.ViewModels;
using lokqlDxComponents;
using lokqlDxComponents.Views.Dialogs;

#pragma warning disable VSTHRD100

namespace LokqlDx.Views;

public partial class QueryEditorView : UserControl
{
    private EditorHelper? _editorHelper;
    private CompletionManager? _completionManager;
    private TextEditor? _textEditor;

    public QueryEditorView()
    {
        InitializeComponent();
        _textEditor = this.FindControl<TextEditor>("TextEditor");
        if (_textEditor != null)
        {
            _editorHelper = new EditorHelper(_textEditor);
        }
        Messaging.RegisterForEvent<ThemeChangedMessage>(this, OnThemeChanged);
    }

    private void OnThemeChanged()
    {
        if (_textEditor != null)
            HighlightHelper.ApplySyntaxHighlighting(_textEditor);
    }

    private QueryEditorViewModel? GetVm() =>
        (DataContext as QueryEditorViewModel)!;


    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        if (_textEditor == null) return;
        
        HighlightHelper.ApplySyntaxHighlighting(_textEditor);
        //Ensure we can intercept ENTER key
        _textEditor.AddHandler(KeyDownEvent, InternalEditor_OnKeyDown, RoutingStrategies.Tunnel);
        _textEditor.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        _textEditor.AddHandler(DragDrop.DragOverEvent, DragOver);
        _textEditor.AddHandler(DragDrop.DropEvent, Drop);
        _textEditor.TextArea.TextEntered += TextArea_TextEntered;
        _textEditor.TextArea.TextView.LinkTextUnderline = true;
        _textEditor.TextArea.TextView.LinkTextForegroundBrush = Brushes.LightGreen;
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
    {
        if (_editorHelper != null)
            DragDropManager.Drop(e, _editorHelper);
    }


    private async void InternalEditor_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (GetVm() is not { } vm || _editorHelper == null)
            return;
        await QueryExecutionHelper.HandleKeyCombo(e, _editorHelper, vm);
        vm.EditorOffset = _editorHelper.CurrentOffset();
    }


    private void TextEditor_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (GetVm() is not { } vm || _textEditor == null || _editorHelper == null)
            return;
        _completionManager = new CompletionManager(_textEditor, _editorHelper, vm,
            new CompletionWindowWrapper(_textEditor.TextArea));
        _textEditor.TextArea.Focus();
    }

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e) => _textEditor?.Focus();
}
