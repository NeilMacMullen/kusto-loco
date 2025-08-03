using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class PiFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 0);
        return new ScalarResult(ScalarTypes.Real, Math.PI);
    }
    //we never call this in columnar because it takes no arguments
    //and we shortcut to creating singlevalue column
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        => throw new NotSupportedException();
}
