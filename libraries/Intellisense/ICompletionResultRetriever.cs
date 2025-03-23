namespace Intellisense;

/// <summary>
/// Interface for a completion result retriever.
/// </summary>
internal interface ICompletionResultRetriever<in T>
{
    CompletionResult GetCompletionResult(T argument);
}
