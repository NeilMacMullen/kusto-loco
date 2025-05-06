using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Strcmp")]
internal partial class StrcmpFunction
{
    public long Impl(string a,string b)
    {
        return Math.Sign(string.CompareOrdinal(a,b));
    }
}
