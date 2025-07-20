namespace Intellisense.FileSystem.Paths;

public static class PathExtensions
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

    public static string NormalizePath(this string path)
    {
        ReadOnlySpan<char> separators = new([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        var span = new Span<char>(new char[path.Length]);
        path.AsSpan().Replace(span, Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return span.TrimEnd(separators).ToString();
    }
}
