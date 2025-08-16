using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public readonly record struct CommandContext(InteractiveTableExplorer Explorer, BlockSequence Sequence)
    : ICommandContext
{
    public IKustoConsole Console => Explorer._outputConsole;
    public KustoQueryContext QueryContext => Explorer.GetCurrentContext();
    public KustoSettingsProvider Settings => Explorer.Settings;
    public IInputProcessor InputProcessor { get; } = new InputProcessor(Sequence,Explorer);
    

    public async Task InjectResult(KustoQueryResult result)
    {
        Explorer._resultHistory.Push(result);
        await Explorer._renderingSurface.RenderToDisplay(result);
        Explorer.DisplayResultsToConsole(result);
    }

    public async Task RunInput(string text) =>await Explorer.RunInput(text);
    public IResultHistory History { get; } = Explorer._resultHistory;
    public KustoQueryResult FetchResult(string resultName) => Explorer._resultHistory.Fetch(resultName);
}

public readonly record struct InputProcessor(BlockSequence Sequence,InteractiveTableExplorer Explorer) : 
    IInputProcessor
{
    public bool IsComplete => Sequence.Complete;
    public string ConsumeNextBlock() =>Explorer.Interpolate(Sequence.Next());
}
