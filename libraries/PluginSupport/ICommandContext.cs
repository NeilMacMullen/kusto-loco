using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;

namespace KustoLoco.PluginSupport;

/// <summary>
/// Provides access to the context in which a command operates, including console, query context, settings, input processor, and result history.
/// </summary>
public interface ICommandContext
{
    /// <summary>
    /// Gets the console for output.
    /// </summary>
    public IKustoConsole Console { get; }

    /// <summary>
    /// Gets the current query context.
    /// </summary>
    public KustoQueryContext QueryContext { get; }

    /// <summary>
    /// Gets the settings provider.
    /// </summary>
    public KustoSettingsProvider Settings { get; }

    /// <summary>
    /// Gets the input processor for handling input blocks.
    /// </summary>
    public IInputProcessor InputProcessor { get; }

    /// <summary>
    /// Injects a query result into the context.
    /// </summary>
    /// <param name="result">The query result to inject.</param>
    public Task InjectResult(KustoQueryResult result);

    /// <summary>
    /// Runs the specified input text as if it had been typed in.
    /// </summary>
    /// <param name="text">The input text to run.</param>
    public Task RunInput(string text);

    /// <summary>
    /// Gets the result history for the context.
    /// </summary>
    public IResultHistory History { get; }
}
