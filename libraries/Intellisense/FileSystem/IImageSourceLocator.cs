using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface IImageSourceLocator
{
    Uri GetImageSource(IFileSystemInfo fileSystemInfo);
}

public class NullImageSourceLocator : IImageSourceLocator
{
    public Uri GetImageSource(IFileSystemInfo fileSystemInfo) => UriExtensions.EmptyUri;
}