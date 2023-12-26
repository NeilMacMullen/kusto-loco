// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class ReplaceStringFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);
        var text = (string?)arguments[0].Value;
        var lookup = (string?)arguments[1].Value;
        var rewrite = (string?)arguments[2].Value;

        return new ScalarResult(ScalarTypes.String, Impl(text, lookup, rewrite));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);
        var textCol = (TypedBaseColumn<string?>)arguments[0].Column;
        var lookupCol = (TypedBaseColumn<string?>)arguments[1].Column;
        var rewriteCol = (TypedBaseColumn<string?>)arguments[2].Column;

        var data = new string?[textCol.RowCount];
        for (var i = 0; i < textCol.RowCount; i++)
        {
            data[i] = Impl(textCol[i], lookupCol[i], rewriteCol[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.String, data));
    }

    private static string? Impl(string? text, string? lookup, string? rewrite)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(lookup))
        {
            return text;
        }

        return text.Replace(lookup, rewrite ?? string.Empty);
    }
}