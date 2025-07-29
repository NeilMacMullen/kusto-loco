// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class LogicalAndOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (bool?)arguments[0].Value;
        var right = (bool?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Bool, WeirdAnd(left, right));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);

        var left = (TypedBaseColumn<bool?>)arguments[0].Column;
        var right = (TypedBaseColumn<bool?>)arguments[1].Column;
        //short-circuiting for indexed columns
        if (left.IsSingleValue && (left[0] == false))
            return new ColumnarResult(left);
        if (right.IsSingleValue && (right[0] == false))
            return new ColumnarResult(right);

        var data = NullableSetBuilderOfbool.CreateFixed(left.RowCount);
        for (var i = 0; i < left.RowCount; i++)
        {
            data.Add(WeirdAnd(left[i], right[i]));
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
    }

    // Nulls are treated as "unknown/any" for logical operations in Kusto.
    // That means that we can short-circuit some combinations
    // but not others
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
    private static bool? WeirdAnd(bool? left, bool? right) =>
        left.HasValue && right.HasValue
            ? left.Value && right.Value
            : (left.HasValue && !left.Value) || (right.HasValue && !right.Value)
                ? false
                : null;
}
