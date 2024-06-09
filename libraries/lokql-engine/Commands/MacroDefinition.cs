namespace Lokql.Engine.Commands;

/// <summary>
/// Defines a macro that can be used in the explorer context
/// </summary>
public class MacroDefinition
{
    private readonly string[] _blocks;

    public MacroDefinition(string name, IEnumerable<string> parameterNames,  string [] blocks)
    {
        Name = name;
        ParameterNames = parameterNames;
        _blocks = blocks;
    }

    public readonly string Name;
    public readonly IEnumerable<string> ParameterNames;
    public BlockSequence Sequence() => new BlockSequence(_blocks);
}
