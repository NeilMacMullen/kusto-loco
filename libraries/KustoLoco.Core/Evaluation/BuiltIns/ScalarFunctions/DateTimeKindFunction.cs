using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "datetime_kind")]
internal partial class DateTimeKindFunction
{
    private static string Impl(DateTime date)
    {
        return date.Kind.ToString();
    }
}
