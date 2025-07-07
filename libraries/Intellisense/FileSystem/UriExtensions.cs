namespace Intellisense.FileSystem;

public static class UriExtensions
{
    public static readonly Uri EmptyUri = new("about:blank");
    public static bool IsEmpty(this Uri uri) => uri.Equals(EmptyUri);
}