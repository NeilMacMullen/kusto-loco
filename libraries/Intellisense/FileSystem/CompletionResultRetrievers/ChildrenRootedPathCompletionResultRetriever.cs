namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class ChildrenRootedPathCompletionResultRetriever(IFileSystemReader reader)
    : IRootedPathCompletionResultRetriever
{
    public CompletionResult? GetCompletionResult(RootedPath rootedPath)
    {
        if (!rootedPath.Value.EndsWithDirectorySeparator() || rootedPath.Value.GetNonEmptyParentDirectory() is not { } parentDir)
        {
            return null;
        }
        return reader
            .GetChildren(parentDir)
            .ToCompletionResult();
    }
}
