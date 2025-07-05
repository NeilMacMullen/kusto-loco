using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface IImageSourceLocator
{
    string GetImageSource(IFileSystemInfo fileSystemInfo);
}

public class NullImageSourceLocator : IImageSourceLocator
{
    public string GetImageSource(IFileSystemInfo fileSystemInfo) => string.Empty;
}