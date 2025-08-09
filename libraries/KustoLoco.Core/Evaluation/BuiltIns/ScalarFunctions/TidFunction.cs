using System.Threading;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "tid")]
internal partial class TidFunction
{
    private static long Impl(long n) => Thread.CurrentThread.ManagedThreadId;
}
