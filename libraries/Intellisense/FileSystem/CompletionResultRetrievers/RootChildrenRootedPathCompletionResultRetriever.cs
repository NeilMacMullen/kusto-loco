namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class RootChildrenRootedPathCompletionResultRetriever(IFileSystemReader reader)
    : IRootedPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(RootedPath rootedPath)
    {
        return reader.GetChildren(rootedPath.Value).ToCompletionResult();
    }

    public bool CanHandle(RootedPath rootedPath)
    {
        return rootedPath.IsRootDirectory();
    }
}
