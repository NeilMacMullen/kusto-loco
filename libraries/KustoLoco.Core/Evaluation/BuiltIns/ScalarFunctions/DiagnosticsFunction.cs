using System;
using System.Threading;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "diagnostics")]
internal partial class DiagnosticsFunction
{
    private static string Impl(string fmt)
    {
        var ret = "";
        foreach (var c in fmt.Split(':'))
        {
            if (c=="tid")
                ret+=$"tid:{Thread.CurrentThread.ManagedThreadId} " ;
            if (c == "proc")
                ret += $"proc:{Thread.GetCurrentProcessorId()} ";
            if (c == "ticks")
                ret += $"ticks:{DateTime.UtcNow.Ticks} ";
        }

        return ret;
    }
}
