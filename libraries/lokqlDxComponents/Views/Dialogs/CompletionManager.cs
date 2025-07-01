using System.Diagnostics.CodeAnalysis;
using Avalonia.Input;
using AvaloniaEdit;
using lokqlDxComponents.Models;
using lokqlDxComponents.Services;
using lokqlDxComponents.ViewModels;

namespace lokqlDxComponents.Views.Dialogs;

public class CompletionManager : IDisposable
{
    private readonly TextEditor _textEditor;
    private readonly IEditorCursorViewModel _editorHelper;
    private readonly ICompletionManagerServiceLocator _vm;
    private readonly CompletionWindowWrapper _completionWindow;

    public CompletionManager(TextEditor editor,
        IEditorCursorViewModel editorHelper,
        ICompletionManagerServiceLocator vm,
        CompletionWindowWrapper completionWindow
        )
    {
        _textEditor = editor;
        _editorHelper = editorHelper;
        _vm = vm;
        _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        _completionWindow = completionWindow;
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    // ReSharper disable once AsyncVoidMethod
    private async void Caret_PositionChanged(object? sender, EventArgs eventArgs) => await _vm._intellisenseClient.OnCaretPositionChanged();

    private async Task<bool> ShowPathCompletions()
    {
        // Avoid unnecessary IO calls
        if (_completionWindow.IsOpen) return false;

        var currentLineText = _editorHelper.GetCurrentLineText();

        var result = await _vm._intellisenseClient.GetPathCompletions(currentLineText);
        if (result.IsEmpty()) return false;



        _completionWindow.ShowCompletions(new ShowCompletionOptions
        {
            Completions = result.Entries,
            Rewind = result.Filter.Length,
            OnCompletionWindowDataPopulated = completionWindow =>
            {
                // when editor loses focus mid-path and user resumes typing,
                // it won't require a second keypress to select the relevant result
                completionWindow.CompletionList.SelectItem(result.Filter);
                if (!completionWindow.CompletionList.CurrentList.Any())
                    completionWindow.Close();
            }
        });


        return true;


    }

    public async Task HandleKeyDown(TextInputEventArgs e)
    {
        _completionWindow.CloseIfEmpty();

        if (await ShowPathCompletions())
            return;

        var options = e.Text switch
        {
            "." when _editorHelper.TextToLeftOfCaret().TrimStart() is "." => new()
            {
                Completions = _vm.InternalCommands
            },
            "|" => new()
            {
                Completions = _vm.KqlOperatorEntries,
                Prefix = " "
            },
            "@" => new()
            {
                Completions = _vm.GetColumns(_editorHelper.GetTextAroundCursor()),
                Rewind = 1
            },
            "[" => new()
            {
                Completions = _vm.GetTables(_editorHelper.GetTextAroundCursor()),
                Rewind = 1
            },
            "$" => new()
            {
                Completions = _vm.SettingNames
            },
            "?" => new()
            {
                Completions = _vm.KqlFunctionEntries,
                Rewind = 1
            },
            _ => ShowCompletionOptions.Empty
        };

        _completionWindow.ShowCompletions(options);

    }


    public void Dispose()
    {
        _textEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        _completionWindow.Close();
    }
}