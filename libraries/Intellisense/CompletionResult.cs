using System.Collections.Immutable;
using System.IO.Abstractions;

namespace Intellisense;

public record CompletionResult
{
    public static readonly CompletionResult Empty = new();
    /// <summary>
    /// List of available completion entries.
    /// </summary>
    public IEnumerable<IntellisenseEntry> Entries { get; init; } = ImmutableArray<IntellisenseEntry>.Empty;
    /// <summary>
    /// The text filter to be applied to the completion entries.
    /// </summary>
    public string Filter { get; init; } = string.Empty;
}

internal static class CompletionResultExtensions
{
    public static CompletionResult ToCompletionResult(this IEnumerable<IFileSystemInfo> files)
    {
        return new CompletionResult
        {
            Entries = files.Select(x => new IntellisenseEntry { Name = x.Name }),
        };
    }
}
