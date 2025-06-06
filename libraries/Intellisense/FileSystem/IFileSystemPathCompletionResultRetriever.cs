using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem;

public interface IFileSystemPathCompletionResultRetriever
{
    Task<CompletionResult> GetSiblingsAsync(FileSystemPath path);
    Task<CompletionResult> GetChildrenAsync(FileSystemPath path);
}
