using AvaloniaEdit.CodeCompletion;
using Intellisense;

namespace lokqlDxComponents.Models;

public readonly record struct CompletionRequest()
{
    public IReadOnlyCollection<IntellisenseEntry> Completions { get; init; } = [];
    public string Prefix { get; init; } = string.Empty;
    public int Rewind { get; init; }
    public Action<CompletionWindow> OnCompletionWindowDataPopulated { get; init; } = EmptyAction;
    private static readonly Action<CompletionWindow> EmptyAction = _ => { };
    public static CompletionRequest Empty => new();
    public bool IsEmpty => Completions.Count is 0;
}
