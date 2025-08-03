using System;
using System.Diagnostics;
using System.Threading;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "partest")]
internal partial class ParTest
{
    private static string Impl(long input)
    {
        var now = DateTime.UtcNow.Ticks;
        var watch = Stopwatch.StartNew();
        while (watch.ElapsedTicks < input) ;
        return $"{Thread.CurrentThread.ManagedThreadId}:{now}";
    }
}
