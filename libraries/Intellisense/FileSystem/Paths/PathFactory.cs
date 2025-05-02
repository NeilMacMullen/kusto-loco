using System.Runtime.InteropServices;

namespace Intellisense.FileSystem.Paths;

internal interface IPathFactory
{
    IFileSystemPath Create(string path);
    T CreateOrThrow<T>(string path) where T : IFileSystemPath;
}

internal class PathFactory : IPathFactory
{
    public IFileSystemPath Create(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            return EmptyPath.Instance;
        }


        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsUnc)
        {
            if (!isWindows)
            {
                return EmptyPath.Instance;
            }

            return new UncPath(uri);
        }

        if (isWindows)
        {
            return new WindowsRootedPath(path);
        }

        return new UnixRootedPath(path);
    }

    public T CreateOrThrow<T>(string path) where T : IFileSystemPath
    {
        var res = Create(path);
        if (res is not T rootedPath)
        {
            throw new ArgumentException($"Failed to create {nameof(T)} from {path}. OS: {Environment.OSVersion}");
        }

        return rootedPath;
    }
}
