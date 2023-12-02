// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Kusto.Language.Parsing;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class ContainsOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (string?)arguments[0].Value;
        var right = (string?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Bool,
            (left ?? string.Empty).ToUpperInvariant().Contains((right ?? string.Empty).ToUpperInvariant()));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (Column<string?>)(arguments[0].Column);
        var right = (Column<string?>)(arguments[1].Column);
        var data = new bool?[left.RowCount];

        var rangePartitioner = Partitioner.Create(0, left.RowCount,1000);

        Parallel.ForEach(rangePartitioner, (range, loopState) =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                var lefts = left[i];
                var rights = right[i];
                data[i] = string.IsNullOrEmpty(rights) ? true
                    : string.IsNullOrEmpty(lefts) ? 
                        false
                    :
                data[i] = lefts!.Contains(rights!,StringComparison.InvariantCultureIgnoreCase);
            }
        });
      

        return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
    }
}