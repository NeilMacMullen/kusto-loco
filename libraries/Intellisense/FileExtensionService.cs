using System.IO.Abstractions;

namespace Intellisense;

public interface IFileExtensionService
{
    IntellisenseHint GetIntellisenseHint(IFileSystemInfo fileSystemInfo);
    IEnumerable<IntellisenseHint> GetFileExtensions();

}

public class FileExtensionService : IFileExtensionService
{
    private readonly Dictionary<string, IntellisenseHint> _fileExtensionMapping;

    public FileExtensionService()
    {
        _fileExtensionMapping = GetFileExtensions()
            .ToDictionary(x => "." + x, x => x, StringComparer.OrdinalIgnoreCase);
    }

    public IntellisenseHint GetIntellisenseHint(IFileSystemInfo fileSystemInfo) => fileSystemInfo is IDirectoryInfo
        ? IntellisenseHint.Folder
        : _fileExtensionMapping.GetValueOrDefault(fileSystemInfo.Extension, IntellisenseHint.File);


    public IEnumerable<IntellisenseHint> GetFileExtensions() =>
        Enum
            .GetValues<IntellisenseHint>()
            .SkipWhile(x => x <= IntellisenseHint.Folder);
}