// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class LogicalOrOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (bool?)arguments[0].Value;
            var right = (bool?)arguments[1].Value;
            return new ScalarResult(ScalarTypes.Bool, WeirdOr(left, right));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<bool?>)(arguments[0].Column);
            var right = (Column<bool?>)(arguments[1].Column);

            var data = new bool?[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = WeirdOr(left[i], right[i]);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }

        // Null handling is weird in real Kusto. Observations:
        //
        // Query:
        // let nil=tobool("");
        // union
        //     (print a=nil, b=nil),
        //     (print a=nil, b=false),
        //     (print a=nil, b=true)
        // | project a, b, AandB = a and b, AorB = a or b
        //
        // Result:
        //
        // a:bool; b: bool; AandB:bool; AorB:bool
        // --------------------------------------
        //       ;        ;           ;
        //       ; false  ; false     ;
        //       ; true   ;           ; true
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool? WeirdOr(bool? left, bool? right)
        {
            return
                (left.HasValue && right.HasValue)
                ? (left.Value || right.Value)
                : ((left.HasValue && left.Value) || (right.HasValue && right.Value))
                    ? true
                    : null;
        }
    }
}
