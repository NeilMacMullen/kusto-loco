namespace KustoLoco.PluginSupport;

/// <summary>
/// Represents a command plugin for Lokql, allowing registration of commands with a command processor.
/// </summary>
public interface ILokqlCommand : ILokqlPlugin
{
    /// <summary>
    /// Registers the command with the specified command processor.
    /// </summary>
    /// <param name="processor">The command processor to register with.</param>
    /// <returns>The command processor with the command registered.</returns>
    public ICommandProcessor Register(ICommandProcessor processor);
}
