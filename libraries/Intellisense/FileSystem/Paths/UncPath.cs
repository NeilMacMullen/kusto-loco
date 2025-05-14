using System.Net;
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

    private bool HasValidHost() => Uri.CheckHostName(uri.Host) is not UriHostNameType.Unknown;

    public bool IsShare() => uri.Segments.Length is 2;

    public string Host
    {
        get
        {
            // uri.Host changes 127 => 127.0.0.0
            // we want to preserve original for autocompletion with partial ip addresses
            var host = uri.Host;
            if (uri.Segments.Length is not 1)
            {
                return host;
            }
            if (!IPAddress.TryParse(host, out _))
            {
                return host;
            }

            return uri.OriginalString.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }

    public string Share => uri.Segments is not [_, var share] ? string.Empty : share.Trim(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
}