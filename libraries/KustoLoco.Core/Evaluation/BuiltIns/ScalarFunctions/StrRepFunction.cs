using System;
using System.Text;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Strrep")]
internal partial class StrRepFunction
{
    public string Impl(string s, long count)
    {
        var sb = new StringBuilder();
        count = Math.Min(count, 1024L);
        for (var i = 0; i < count; i++)
            sb.Append(s);
        return sb.ToString();
    }
}
