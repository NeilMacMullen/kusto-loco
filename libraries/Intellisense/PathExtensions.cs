namespace Intellisense;

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

    public static string? GetNonEmptyParentDirectory(this string path)
    {
        var res = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(res))
        {
            return null;
        }

        return res;
    }

    public static string GetNonEmptyParentDirectoryOrThrow(this string path)
    {

        if (path.GetNonEmptyParentDirectory() is not {} parent)
        {
            throw new ArgumentException($"Could not get parent path from path {path}");
        }

        return parent;
    }

    public static string? GetNonEmptyFileName(this string path)
    {
        var res = Path.GetFileName(path);
        if (string.IsNullOrEmpty(res))
        {
            return null;
        }

        return res;
    }

    public static string GetNonEmptyFileNameOrThrow(this string path)
    {
        if (path.GetNonEmptyFileName() is not {} res)
        {
            throw new ArgumentException($"Could not get nonempty file name from path {path}");
        }

        return res;
    }
}
