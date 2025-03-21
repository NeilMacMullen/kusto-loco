namespace Intellisense.FileSystem.Paths;

internal static class PathExtensions
{
    public static bool EndsWithDirectorySeparator(this string path)
    {
        return path.Length > 0 && path[^1].IsDirectorySeparator();
    }

    public static bool IsDirectorySeparator(this string path)
    {
        return path.Length is 1 && path[0].IsDirectorySeparator();
    }

    public static bool IsDirectorySeparator(this char path)
    {
        return path == Path.DirectorySeparatorChar || path == Path.AltDirectorySeparatorChar;
    }

    public static bool IsValidDriveChar(this char value)
    {
        // copied from Path internals
        return (uint)((value | 0x20) - 'a') <= 'z' - 'a';
    }
}
