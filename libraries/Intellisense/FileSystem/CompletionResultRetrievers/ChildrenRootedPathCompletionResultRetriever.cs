namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class ChildrenRootedPathCompletionResultRetriever(IFileSystemReader reader)
    : IRootedPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(RootedPath rootedPath)
    {
        return reader
            .GetChildren(rootedPath.Value.GetNonEmptyParentDirectoryOrThrow())
            .ToCompletionResult();
    }

    public bool CanHandle(RootedPath rootedPath)
    {
        return rootedPath.Value.EndsWithDirectorySeparator() && rootedPath.Value.GetNonEmptyParentDirectory() is not null;
    }
}
