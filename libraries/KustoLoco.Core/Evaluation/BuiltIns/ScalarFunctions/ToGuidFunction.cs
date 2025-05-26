using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToGuid")]
internal partial class ToGuidFunction
{
    private static Guid GuidImpl(Guid input) => input;

    private static Guid? StringImpl(string input)
    {
        return Guid.TryParse(input.Replace("-",""), out var parsedResult)
            ? parsedResult
            : null;
    }
}
