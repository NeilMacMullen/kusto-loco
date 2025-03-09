using System.Collections.Immutable;

namespace Intellisense;

public record CompletionResult
{
    public static readonly CompletionResult Empty = new();

    public IEnumerable<IntellisenseEntry> Entries { get; init; } = ImmutableArray<IntellisenseEntry>.Empty;
    public string Prefix { get; init; } = string.Empty;
    public int Rewind { get; init; }
}
