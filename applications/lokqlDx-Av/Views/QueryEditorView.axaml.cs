using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Intellisense;
using lokqlDx;
using LokqlDx.ViewModels;
using NotNullStrings;
using System.Xml;
using Avalonia.Media;

#pragma warning disable VSTHRD100

namespace LokqlDx.Views;

public partial class QueryEditorView : UserControl
{
    private readonly EditorHelper _editorHelper;

    public QueryEditorView()
    {
        InitializeComponent();
        _editorHelper = new EditorHelper(TextEditor);
    }

    private QueryEditorViewModel GetVm() =>
        (DataContext as QueryEditorViewModel)!;


    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        using var s = ResourceHelper.SafeGetResourceStream("SyntaxHighlighting.xml");
        using var reader = new XmlTextReader(s);
        TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        //Ensure we can intercept ENTER key
        TextEditor.AddHandler(KeyDownEvent, InternalEditor_OnKeyDown, RoutingStrategies.Tunnel);
        TextEditor.AddHandler(TextInputEvent,docompletion, RoutingStrategies.Tunnel);
        TextEditor.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        TextEditor.AddHandler(DragDrop.DragOverEvent, DragOver);

        TextEditor.AddHandler(DragDrop.DropEvent, Drop);
    }

      private void DragOver(object? sender, DragEventArgs drgevent)
    {
        drgevent.Handled = true;
        // Check that the data being dragged is a file
        drgevent.DragEffects = drgevent.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Link
            : DragDropEffects.None;
    }

    private void DragEnter(object? sender, DragEventArgs drgevent)
    {
        drgevent.Handled = true;
        // Check that the data being dragged is a file
        drgevent.DragEffects = drgevent.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Link
            : DragDropEffects.None;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var newString = e.Data.GetFiles()!
                .Select(s => s.TryGetLocalPath().NullToEmpty())
                .Where(s => s.IsNotBlank())
                .Select(f => $".{VerbFromExtension(f)} \"{f}\"")
                .JoinAsLines();
                 _editorHelper.InsertAtCursor(newString);
        }

        e.Handled = true;
        return;

        string VerbFromExtension(string f)
        {
            return f.EndsWith(".csl")
                ? "run"
                : "load";
        }
    }
    

    private async void InternalEditor_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var isEnter = e.Key is Key.Enter;
        var shiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var ctrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);

        if (isEnter && shiftDown)
        {
            e.Handled = true;
            var query = ctrlDown
                ? _editorHelper.GetFullText()
                : _editorHelper.GetTextAroundCursor();

            await GetVm().RunQueryCommand.ExecuteAsync(query);
        }

        if (e.Key == Key.Down && ctrlDown)
        {
            e.Handled = true;
            _editorHelper.ScrollDownToComment();
        }

        if (e.Key == Key.Up && ctrlDown)
        {
            e.Handled = true;
            _editorHelper.ScrollUpToComment();
        }
    }

    private CompletionWindow? _completionWindow;
    private void ShowCompletions(IReadOnlyList<IntellisenseEntry> completions, string prefix, int rewind,
        Action<CompletionWindow>? onCompletionWindowDataPopulated = null)
    {
        if (!completions.Any())
            return;

        _completionWindow = new CompletionWindow(TextEditor.TextArea)
        {
            CloseWhenCaretAtBeginning = true
        };
        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
        foreach (var k in completions.OrderBy(k => k.Name))
            data.Add(new MyCompletionData(k, prefix, rewind));

        _completionWindow.Closed += delegate { _completionWindow = null; };
        onCompletionWindowDataPopulated?.Invoke(_completionWindow);
        _completionWindow?.Show();
    }

    private  void docompletion(object? sender, TextInputEventArgs e)
    {
        if (_completionWindow != null && !_completionWindow.CompletionList.CurrentList.Any())
        {
            _completionWindow.Close();
            return;
        }

        //if (await ShowPathCompletions()) return;

        if (e.Text == ".")
        {
            //only show completions if we are at the start of a line
            var textToLeft = _editorHelper.TextToLeftOfCaret();
            if (textToLeft.TrimStart() == ".") ShowCompletions(
                //_internalCommands,
                GetVm()._kqlFunctionEntries,
                string.Empty, 0);
            return;
        }

        if (e.Text == "|")
        {
            ShowCompletions(
                GetVm()._kqlFunctionEntries,
                //KqlOperatorEntries,
                " ", 0);
            return;
        }

        if (e.Text == "@")
        {
          //  var blockText = GetTextAroundCursor();
          //  var columns = _schemaIntellisenseProvider.GetColumns(blockText);
          //  ShowCompletions(columns, string.Empty, 1);
            return;
        }

        if (e.Text == "[")
        {
          //  var blockText = GetTextAroundCursor();
         //   var tables = _schemaIntellisenseProvider.GetTables(blockText);
         //   ShowCompletions(tables, string.Empty, 1);
            return;
        }

        if (e.Text == "$")
        {
       //     ShowCompletions(_settingNames, string.Empty, 0);
            return;
        }

        if (e.Text == "?") ShowCompletions(
            //_kqlFunctionEntries,
            GetVm()._kqlFunctionEntries,
            string.Empty, 1);
    }


    private async Task<bool> ShowPathCompletions()
    {
        // Avoid unnecessary IO calls
        if (_completionWindow is not null) return false;
        /*
        var path = Parser.GetLastArgument(_editorHelper.GetCurrentLineText());

        if (path.IsBlank()) return false;

        var result = await _intellisenseClient.GetCompletionResultAsync(path);
        if (result.IsEmpty()) return false;
        */
        ShowCompletions(
            GetVm()._kqlFunctionEntries,
            string.Empty,
            1,
            completionWindow =>
            {
                // when editor loses focus mid-path and user resumes typing, it won't require a second keypress to select the relevant result
                //completionWindow.CompletionList.SelectItem(result.Filter);
                if (!completionWindow.CompletionList.CurrentList.Any()) completionWindow.Close();
            });
        await Task.CompletedTask;
        return true;
    }
}


/// Implements AvalonEdit ICompletionData interface to provide the entries in the
/// completion drop down.
public class MyCompletionData(IntellisenseEntry entry, string prefix, int rewind) : ICompletionData
{
    public string Text => entry.Name;

    // Use this property if you want to show a fancy UIElement in the list.
    public object Content => Text;

    public object Description
        => entry.Syntax.IsBlank()
            ? entry.Description
            : $@"{entry.Description}
Usage: {entry.Syntax}";


    public double Priority => 1.0;

    public void Complete(TextArea textArea, ISegment completionSegment,
        EventArgs insertionRequestEventArgs)
    {
        var seg = new TextSegment
        {
            StartOffset = completionSegment.Offset - rewind,
            Length = completionSegment.Length + rewind
        };
        textArea.Document.Replace(seg, prefix + Text);
    }

    public IImage? Image { get; } = null;
}
