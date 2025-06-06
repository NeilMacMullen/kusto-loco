﻿using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class PiFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 0);
        return new ScalarResult(ScalarTypes.Real, Math.PI);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        => throw new NotSupportedException();
}
