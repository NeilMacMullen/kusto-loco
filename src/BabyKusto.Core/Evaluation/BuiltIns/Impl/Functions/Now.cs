// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class NowFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 0);
            return new ScalarResult(ScalarTypes.DateTime, DateTime.UtcNow);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            throw new NotSupportedException();
        }
    }
}
