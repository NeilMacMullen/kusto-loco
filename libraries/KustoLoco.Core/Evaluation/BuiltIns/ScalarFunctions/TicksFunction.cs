using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "ticks")]
internal partial class TicksFunction
{
    private static long Impl(long n) => DateTime.UtcNow.Ticks;
}
