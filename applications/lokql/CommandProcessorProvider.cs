using KustoLoco.PluginSupport;
using Lokql.Engine.Commands;

/// <summary>
///     Indirection class to allow cleaner source-level extension of the CommandProcessor
/// </summary>
public static class CommandProcessorProvider
{
    public static CommandProcessor GetCommandProcessor() => CommandProcessor.Default();
    //Add .LegacyWithAdditionalCommand lines here to extend the processor
}
