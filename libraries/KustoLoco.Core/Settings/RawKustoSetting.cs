namespace KustoLoco.Core.Settings;

public record RawKustoSetting(string Name, string Value)
{
    public string Key => Name.ToLowerInvariant();
}
