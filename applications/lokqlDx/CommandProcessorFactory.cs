using Lokql.Engine.Commands;

namespace LokqlDx;

public class CommandProcessorFactory
{
    public CommandProcessor GetCommandProcessor() => CommandProcessor.Default();
}
