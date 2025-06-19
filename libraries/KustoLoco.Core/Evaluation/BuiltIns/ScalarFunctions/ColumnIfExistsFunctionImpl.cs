using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class ColumnIfExistsFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) => throw new NotImplementedException();

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        throw new NotImplementedException();
    }
}
