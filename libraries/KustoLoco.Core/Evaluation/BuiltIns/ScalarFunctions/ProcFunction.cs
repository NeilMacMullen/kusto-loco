
using System.Threading;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "proc")]
internal partial class ProcFunction
{
    private static long Impl(long n) => Thread.GetCurrentProcessorId();
}
