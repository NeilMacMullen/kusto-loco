namespace KustoLoco.PluginSupport;

/// <summary>
/// ICommandProcessor is the entity that calls plugin commands as appropriate
/// </summary>
/// <remarks>
/// Plugins must register a callback task and an options Type which
/// is filled with values parsed from the command line
/// </remarks>
public interface ICommandProcessor
{
    /// <summary>
    /// Returns a new command processor with an additional command handler.
    /// </summary>
    /// <typeparam name="T">The type of the commands Options argument.</typeparam>
    /// <param name="runFunction">The function to execute for the command.</param>
    /// <returns>A new <see cref="ICommandProcessor"/> instance with the additional command.</returns>
    ICommandProcessor WithAdditionalCommand<T>(Func<ICommandContext, T, Task> runFunction);

    /// <summary>
    /// Allows a plugin command to register a schema for expected tables.
    /// </summary>
    /// <remarks>
    /// This is used, for example, by .appinsights because the names
    /// of available tables and columns are already known so can be supplied
    /// to intellisense.
    ///
    /// The schemaText must be a series of lines, each containing
    /// a tablename,column pair.
    /// </remarks>
    /// <param name="command">The command name.</param>
    /// <param name="schemaText">The schema definition as text.</param>
    void RegisterSchema(string command, string schemaText);
}
