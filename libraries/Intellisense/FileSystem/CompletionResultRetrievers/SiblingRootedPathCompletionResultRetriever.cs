namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class SiblingRootedPathCompletionResultRetriever(IFileSystemReader reader) : IRootedPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(RootedPath rootedPath)
    {
        var pair = ParentChildPathPair.CreateOrThrow(rootedPath.Value);

        return reader
            .GetChildren(pair.ParentPath)
            .ToCompletionResult() with { Filter = pair.CurrentPath };
    }

    public bool CanHandle(RootedPath rootedPath)
    {
        return !rootedPath.IsRootDirectory() && !rootedPath.Value.EndsWithDirectorySeparator();
    }
}
