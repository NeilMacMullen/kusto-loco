namespace Intellisense;

/// <summary>
/// Interface for a completion result retriever.
/// </summary>
internal interface ICompletionResultRetriever<in T>
{
    Task<CompletionResult> GetCompletionResultAsync(T argument);
}
