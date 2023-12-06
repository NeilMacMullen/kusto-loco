using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class ColumnIfExistsFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) => throw new NotImplementedException();

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columnName = arguments[0];
        throw new NotImplementedException();
    }
}