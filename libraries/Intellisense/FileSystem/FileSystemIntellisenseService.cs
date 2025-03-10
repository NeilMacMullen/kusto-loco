using System.Diagnostics;
using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(string path);
}

public class FileSystemIntellisenseService(IFileSystem fileSystem) : IFileSystemIntellisenseService
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true
    };

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        if (!fileSystem.Path.IsPathRooted(path))
        {
            return CompletionResult.Empty;
        }

        if (IsRoot(path))
        {
            if (path.EndsWith(':'))
            {
                return CompletionResult.Empty;
            }

            return new CompletionResult
            {
                Entries = GetOptionsFromFileSystem(path)
            };
        }

        if (IsDirectory(path))
        {
            if (fileSystem.Path.EndsInDirectorySeparator(path))
            {
                return new CompletionResult
                {
                    Entries = GetOptionsFromFileSystem(path)
                };
            }

            if (GetDirAndFileNames(path) is not { } pair2)
            {
                throw new UnreachableException($"Did not expect to fail to retrieve dir and file name for path {path}");
            }

            return new CompletionResult
            {
                Entries = GetOptionsFromFileSystem(pair2.DirName),
                Rewind = pair2.FileName.Length
            };
        }

        if (GetDirAndFileNames(path) is not { } pair)
        {
            return CompletionResult.Empty;
        }
        var entries = GetOptionsFromFileSystem(pair.DirName).Where(x => x.Name.Contains(pair.FileName,StringComparison.CurrentCultureIgnoreCase));

        return new CompletionResult
        {
            Entries = entries,
            Rewind = pair.FileName.Length
        };
    }

    private bool IsDirectory(string path)
    {
        if (fileSystem.Directory.Exists(path))
        {
            return true;
        }
        if (path is "/" or "\\")
        {
            return fileSystem.Directory.Exists("C:/");
        }

        return false;
    }

    private (string DirName, string FileName)? GetDirAndFileNames(string path)
    {
        if (fileSystem.Path.GetDirectoryName(path) is not { } dirPath)
        {
            return null;
        }

        if (!IsDirectory(dirPath))
        {
            return null;
        }

        var fileName = fileSystem.Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return (dirPath, fileName);
    }

    private bool IsRoot(string path)
    {
        return fileSystem.Path.IsPathRooted(path) && fileSystem.Path.GetDirectoryName(path) is null && IsDirectory(path);
    }

    private IEnumerable<IntellisenseEntry> GetOptionsFromFileSystem(string dirPath)
    {
        return fileSystem
            .DirectoryInfo.New(dirPath)
            .EnumerateFileSystemInfos("*", EnumerationOptions)
            .Select(x => new IntellisenseEntry { Name = x.Name });
    }
}
