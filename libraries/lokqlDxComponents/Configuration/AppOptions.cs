namespace lokqlDxComponents.Configuration;

public record AppOptions
{
    public required string AssemblyName { get; set; }
    public Uri BaseUri => new($"avares://{AssemblyName}");
    public Uri AssetsUri => new(BaseUri, "Assets/");
    public Uri CompletionIconsUri => new(AssetsUri, "CompletionIcons/");
}
