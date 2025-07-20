using Avalonia.Input;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using lokqlDxComponents.Events;
using lokqlDxComponents.Models;
using lokqlDxComponents.ViewModels;

namespace lokqlDxComponents.Views.Dialogs;

public class HandleKeyDownMessage : AsyncRequestMessage<ShowCompletionOptions>
{
    public required IEditorCursorViewModel Cursor { get; init; }
    public required string Text { get; init; }
    public required bool IsCompletionWindowOpen { get; init; }
}

public class CompletionManager : IDisposable
{
    private readonly TextEditor _textEditor;
    private readonly IEditorCursorViewModel _editorHelper;
    private readonly CompletionWindowWrapper _completionWindow;
    private readonly Func<IMessenger> _messenger;

    public CompletionManager(
        TextEditor editor,
        IEditorCursorViewModel editorHelper,
        Func<IMessenger> messenger,
        CompletionWindowWrapper completionWindow
    )
    {
        _textEditor = editor;
        _editorHelper = editorHelper;
        _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        _completionWindow = completionWindow;
        _messenger = messenger;
    }


    private void Caret_PositionChanged(object? sender, EventArgs eventArgs) =>
        _messenger().Send<CaretPositionChangedMessage>();


    public async Task HandleKeyDown(TextInputEventArgs e)
    {
        // don't return early here if it successfully closes, will break nested dir autocompletion i.e. /abc/d/...
        _completionWindow.CloseIfEmpty();

        var result = await _messenger().Send(new HandleKeyDownMessage
            {
                Cursor = _editorHelper,
                IsCompletionWindowOpen = _completionWindow.IsOpen,
                Text = e.Text ?? ""
            }
        );

        _completionWindow.ShowCompletions(result);
    }


    public void Dispose()
    {
        _textEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        _completionWindow.Close();
    }
}
