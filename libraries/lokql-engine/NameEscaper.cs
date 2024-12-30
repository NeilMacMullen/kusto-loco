using KustoLoco.Core;

namespace Lokql.Engine;

//TODO - move this functionality into core library when it is available
public static class NameEscaper
{
    public static string EscapeIfNecessary(string name)
    {
        name = KustoNameEscaping.RemoveFraming(name);
        return name
            .Any(c => !char.IsLetterOrDigit(c))
            ? KustoNameEscaping.EnsureFraming(name)
            : name;
    }
}
