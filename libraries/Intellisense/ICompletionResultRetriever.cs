namespace Intellisense;

/// <summary>
/// Interface for a completion result retriever returning null if it cannot handle the given argument.
/// </summary>
internal interface ICompletionResultRetriever<in T>
{
    CompletionResult? GetCompletionResult(T argument);
}
