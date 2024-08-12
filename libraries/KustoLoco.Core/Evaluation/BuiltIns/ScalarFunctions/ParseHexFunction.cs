using System;
using System.Globalization;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "parsehex")]
internal partial class ParseHexFunction
{
    private static long? Impl(string s)
    {
        if (s.StartsWith("0x",StringComparison.InvariantCultureIgnoreCase))
            s=s.Substring(2);

        return long.TryParse(s, NumberStyles.AllowHexSpecifier,
            CultureInfo.InvariantCulture, out var x)
            ? x
            : null;
    }
}
