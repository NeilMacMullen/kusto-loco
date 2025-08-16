namespace KustoLoco.PluginSupport;

/// <summary>
/// Represents a plugin for Lokql, providing name and version information.
/// </summary>
public interface ILokqlPlugin
{
    /// <summary>
    /// Called by the application to identify the plugin
    /// </summary>
    /// <remarks>
    /// The string returned should be of the form "command version"
    /// E.g. "myplugin v1.0
    /// </remarks>
    /// <returns>The name and version string.</returns>
    public string GetNameAndVersion();
}
