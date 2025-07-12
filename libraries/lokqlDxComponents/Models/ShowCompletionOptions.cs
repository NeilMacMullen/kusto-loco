using AvaloniaEdit.CodeCompletion;

namespace lokqlDxComponents.Models;

public readonly record struct ShowCompletionOptions()
{
    public IReadOnlyCollection<QueryEditorCompletionData> Completions { get; init; } = [];
    public Action<CompletionWindow> OnCompletionWindowDataPopulated { get; init; } = EmptyAction;
    private static readonly Action<CompletionWindow> EmptyAction = _ => { };
}
