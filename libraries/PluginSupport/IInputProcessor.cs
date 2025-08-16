namespace KustoLoco.PluginSupport;

/// <summary>
/// The input processor allows a command to operate on sequential blocks of input
/// </summary>
/// <remarks>
/// A command such as .appinsights needs to consume the following block of text
/// in order to submit it as a query and the IInputProcessor allows this.
/// </remarks>
public interface IInputProcessor
{
    /// <summary>
    /// Gets a value indicating whether the input is complete.
    /// </summary>
    public bool IsComplete { get; }

    /// <summary>
    /// Consumes the next block of input and returns it as a string.
    /// </summary>
    /// <returns>The next input block as a string.</returns>
    public string ConsumeNextBlock();
}
