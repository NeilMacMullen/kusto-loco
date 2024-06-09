namespace Lokql.Engine.Commands;

/// <summary>
/// Holds a set of macro definitions that can be applied in the current explorer context
/// </summary>
public class MacroRegistry
{
    private readonly List<MacroDefinition> _macros = [];

    public void AddMacro(MacroDefinition macro)
    {
        _macros.Add(macro);
    }

    public MacroDefinition GetMacro(string name)
    {
        return _macros.Where(f => f.Name == name)
            .Append(new MacroDefinition(string.Empty, [], []))
            .First();
    }
}
