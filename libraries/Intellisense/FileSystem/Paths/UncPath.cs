using NotNullStrings;

namespace Intellisense.FileSystem.Paths;

internal class UncPath(Uri uri) : IFileSystemPath
{

    public string GetPath() => uri.OriginalString;
    public bool IsRootDirectory() => false;

    public bool IsHost()
    {
        var uriPath = uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
        return uriPath.IsBlank() && HasValidHost();
    }

    private bool HasValidHost() => Uri.CheckHostName(Host) is not UriHostNameType.Unknown;

    public bool IsShare() => uri.Segments.Length is 2;

    public string Host => uri.Host;

    public string Share => uri.Segments[1].TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
}
