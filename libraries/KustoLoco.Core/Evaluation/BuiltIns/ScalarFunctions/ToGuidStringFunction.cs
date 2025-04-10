using System;
using System.Runtime.CompilerServices;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToGuidStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Guid? Impl(string input)
    {
       
        return Guid.TryParse(input.Replace("-",""), out var parsedResult)
            ? parsedResult
            : null;
    }
}
