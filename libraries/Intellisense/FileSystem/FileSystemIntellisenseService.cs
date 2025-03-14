using Intellisense.FileSystem.CompletionResultRetrievers;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(string path);
}

internal class FileSystemIntellisenseService(IFileSystemReader reader)
    : IFileSystemIntellisenseService
{
    private readonly IRootedPathCompletionResultRetriever[] _retrievers =
    [
        new RootChildrenRootedPathCompletionResultRetriever(reader),
        new ChildrenRootedPathCompletionResultRetriever(reader),
        new SiblingRootedPathCompletionResultRetriever(reader)
    ];

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        if (RootedPath.Create(path) is not { } rootedPath)
        {
            return CompletionResult.Empty;
        }
        try
        {
            if (_retrievers.FirstOrDefault(x => x.CanHandle(rootedPath)) is not { } retriever)
            {
                return CompletionResult.Empty;
            }
            return retriever.GetCompletionResult(rootedPath);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine(e);
            return CompletionResult.Empty;
        }
    }
}
