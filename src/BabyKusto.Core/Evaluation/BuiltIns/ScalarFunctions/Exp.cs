// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class ExpFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var value = (double?)arguments[0].Value;

            return new ScalarResult(ScalarTypes.Real, Impl(value));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var valueCol = (Column<double?>)(arguments[0].Column);

            var data = new double?[valueCol.RowCount];
            for (var i = 0; i < valueCol.RowCount; i++)
            {
                var value = valueCol[i];
                data[i] = Impl(value);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }

        private static double? Impl(double? input)
        {
            if (input.HasValue)
            {
                return Math.Exp(input.Value);
            }

            return null;
        }
    }
}
