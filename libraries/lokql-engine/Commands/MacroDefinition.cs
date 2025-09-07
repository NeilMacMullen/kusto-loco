namespace Lokql.Engine.Commands;

/// <summary>
/// Defines a macro that can be used in the explorer context
/// </summary>
public class MacroDefinition
{
    private readonly string[] _blocks;

    public MacroDefinition(string name, IEnumerable<string> parameterNames,  string [] blocks,
        string description)
    {
        Name = name;
        ParameterNames = parameterNames;
        _blocks = blocks;
        Description = description;
    }

    public readonly string Name;
    public readonly IEnumerable<string> ParameterNames;
    public readonly string Description = string.Empty;

    public BlockSequence Sequence() => new(_blocks);
}
