namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class RootChildrenRootedPathCompletionResultRetriever(IFileSystemReader reader)
    : IRootedPathCompletionResultRetriever
{
    public CompletionResult? GetCompletionResult(RootedPath rootedPath)
    {
        if (!rootedPath.IsRootDirectory())
        {
            return null;
        }
        return reader.GetChildren(rootedPath.Value).ToCompletionResult();
    }
}
