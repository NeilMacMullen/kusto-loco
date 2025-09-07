using System.Collections.Immutable;

namespace Lokql.Engine.Commands;

/// <summary>
///     Holds a set of macro definitions that can be applied in the current explorer context
/// </summary>
public class MacroRegistry
{
    private ImmutableDictionary<string, MacroDefinition> _macros = ImmutableDictionary<string, MacroDefinition>.Empty;

    public void AddMacro(MacroDefinition macro)
    {
        _macros = _macros.SetItem(macro.Name, macro);
    }

    public MacroDefinition GetMacro(string name)
    {
        return _macros.TryGetValue(name, out var m)
            ? m
            : new MacroDefinition(string.Empty, [], []);
    }

    public IEnumerable<MacroDefinition> List()
    {
        return _macros.Values;
    }
}
