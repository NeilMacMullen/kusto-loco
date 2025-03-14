namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class RootedPathCompletionResultRetriever(IEnumerable<IRootedPathCompletionResultRetriever> retrievers) : IPathCompletionResultRetriever
{

    public CompletionResult? GetCompletionResult(string path)
    {
        if (RootedPath.Create(path) is not { } rootedPath)
        {
            return null;
        }

        return retrievers.Select(x => x.GetCompletionResult(rootedPath)).FirstOrDefault(x => x is not null);
    }
}
