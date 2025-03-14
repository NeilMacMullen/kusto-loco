namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class SiblingRootedPathCompletionResultRetriever(IFileSystemReader reader) : IRootedPathCompletionResultRetriever
{
    public CompletionResult? GetCompletionResult(RootedPath rootedPath)
    {
        if (ParentChildPathPair.Create(rootedPath.Value) is not { } pair)
        {
            return null;
        }

        return reader
            .GetChildren(pair.ParentPath)
            .ToCompletionResult() with { Filter = pair.CurrentPath };
    }
}
