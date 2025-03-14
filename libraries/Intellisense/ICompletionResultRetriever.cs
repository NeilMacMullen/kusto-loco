namespace Intellisense;

internal interface ICompletionResultRetriever<in T>
{
    CompletionResult GetCompletionResult(T argument);
    bool CanHandle(T argument);
}
