using System.IO.Abstractions;
using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

public class FileSystemCompletionResultRetriever(IFileSystemReader reader, IFileExtensionService service)
    : IFileSystemPathCompletionResultRetriever
{
    public Task<CompletionResult> GetSiblingsAsync(FileSystemPath path)
    {
        var pair = ParentChildPathPair.Create(path.Value);


        var result = new CompletionResult
        {
            Entries = reader
                .GetChildren(pair.ParentPath)
                .Select(CreateIntellisenseEntry)
                .ToList(),
            Filter = pair.CurrentPath
        };

        return Task.FromResult(result);
    }

    public Task<CompletionResult> GetChildrenAsync(FileSystemPath path)
    {
        var input = path.IsRootDirectory ? path.Value : path.ParentPath;

        var result = new CompletionResult
        {
            Entries = reader
                .GetChildren(input)
                .Select(CreateIntellisenseEntry)
                .ToList()
        };

        return Task.FromResult(result);
    }

    private IntellisenseEntry CreateIntellisenseEntry(IFileSystemInfo info) =>
        new()
        {
            Name = info.Name,
            Hint = service.GetIntellisenseHint(info)
        };
}
