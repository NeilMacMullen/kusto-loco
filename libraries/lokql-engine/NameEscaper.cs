using KustoLoco.Core;

namespace Lokql.Engine;

//TODO - move this functionality into core library when it is available
public static class NameEscaper
{
    public static string EscapeIfNecessary(string name)
    {
       return  KustoNameEscaping.EscapeIfNecessary(name);
    }
}
