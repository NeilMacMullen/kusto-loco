using System.Runtime.InteropServices;

namespace Intellisense.FileSystem.Paths;

public interface IPathFactory
{
    FileSystemPath Create(string path);
    T CreateOrThrow<T>(string path) where T : FileSystemPath;
}

public class PathFactory : IPathFactory
{
    public FileSystemPath Create(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            return EmptyPath.Instance;
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsUnc)
        {
            return new UncPath(uri);
        }


        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new WindowsRootedPath(path)
            : new UnixRootedPath(path);
    }

    public T CreateOrThrow<T>(string path) where T : FileSystemPath
    {
        var res = Create(path);
        if (res is not T rootedPath)
        {
            throw new ArgumentException($"Failed to create {nameof(T)} from {path}. OS: {Environment.OSVersion}");
        }

        return rootedPath;
    }
}
