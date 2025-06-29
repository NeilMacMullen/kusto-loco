using Lokql.Engine.Commands;

namespace Lokql.Engine;

/// <summary>
/// Specifies options for file-related commands.
/// Used by the <see cref="CommandProcessor"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FileOptionsAttribute : Attribute
{
    /// <summary>
    /// The allowed file extensions.
    /// Extensions should include the dot prefix (e.g. [".xlsx", ".csv"]).
    /// If this is empty (the default), then all file extensions are allowed.
    /// </summary>
    public string[] Extensions { get; init; } = [];

    /// <summary>
    /// Enable to concatenate existing allowed extensions with those sourced from <see cref="StandardFormatAdaptor"/>
    /// </summary>
    public bool IncludeStandardFormatterExtensions { get; init; }
}
