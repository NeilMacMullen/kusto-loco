namespace Lokql.Engine;

public readonly record struct VerbEntry(string Name, string HelpText, bool SupportsFiles, IReadOnlyList<string> SupportedExtensions);
