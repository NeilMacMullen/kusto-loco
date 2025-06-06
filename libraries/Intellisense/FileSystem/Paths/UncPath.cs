using NotNullStrings;

namespace Intellisense.FileSystem.Paths;

public class UncPath(Uri uri) : FileSystemPath(uri.OriginalString)
{
    /// <summary>
    /// Checks if the UNC path only contains a host i.e. \\127.0.0.1\
    /// </summary>
    public bool IsOnlyHost => uri.Segments.Length is 1;

    public bool HasValidHost => uri.HostNameType is not UriHostNameType.Unknown;

    /// <summary>
    /// Checks if the UNC path contains both the host and share i.e. \\127.0.0.1\share\
    /// </summary>
    public bool IsOnlyHostAndShare => uri.Segments.Length is 2;

    /// <summary>
    /// Retrieves the host segment of the UNC path i.e. "localhost" from \\localhost\share1\
    /// </summary>
    public string OriginalHost
    {
        get
        {
            // uri.Host changes 127 => 127.0.0.0
            // we want to preserve original for autocompletion with partial ip addresses


            if (uri.HostNameType is not (UriHostNameType.IPv4 or UriHostNameType.IPv6))
            {
                return uri.Host;
            }

            return uri
                .OriginalString.Skip(2)
                .TakeWhile(x => x != Path.AltDirectorySeparatorChar && x != Path.DirectorySeparatorChar)
                .JoinString("");
        }
    }

    public string Share => uri.Segments.Length < 2
        ? string.Empty
        : uri.Segments[1].Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    public override bool IsRootDirectory => IsOnlyHostAndShare && EndsWithDirectorySeparator;
}
