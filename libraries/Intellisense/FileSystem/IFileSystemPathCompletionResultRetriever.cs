using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem;

internal interface IFileSystemPathCompletionResultRetriever
{
    Task<CompletionResult> GetSiblingsAsync(FileSystemPath path);
    Task<CompletionResult> GetChildrenAsync(FileSystemPath path);
}
