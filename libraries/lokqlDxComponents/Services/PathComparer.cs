namespace lokqlDxComponents.Services;

public class PathComparer : IEqualityComparer<string>
{
    public static readonly PathComparer Instance = new();

    public bool Equals(string? x, string? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;
        return x == y || x.NormalizePath().Equals(y.NormalizePath(), StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(string obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.NormalizePath());
}


file static class PathExtensions
{
    public static string NormalizePath(this string path)
    {
        var span = new Span<char>(new char[path.Length]);
        path.AsSpan().Replace(span, '\\','/');
        var res = span.Trim('/').ToString();
        return res;
    }
}