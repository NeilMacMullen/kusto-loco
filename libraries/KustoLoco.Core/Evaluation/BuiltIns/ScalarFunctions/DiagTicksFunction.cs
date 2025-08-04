using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "diagTicks")]
internal partial class DiagTicksFunction
{
    private static long Impl(long n) => DateTime.UtcNow.Ticks;
}
