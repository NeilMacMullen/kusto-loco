using Intellisense.FileSystem;

namespace Intellisense;

public record struct IntellisenseEntry(
    string Name = "",
    string Description = "",
    string Syntax = "",
    Uri? Source = null)
{
    public Uri Source = Source ?? UriExtensions.EmptyUri;
}
