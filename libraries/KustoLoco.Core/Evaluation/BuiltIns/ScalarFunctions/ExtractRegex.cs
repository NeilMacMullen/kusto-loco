// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/// <remarks>
///     This uses the built-in .NET Regex implementation, which isn't strictly compatible with Kusto's RE2.
///     See: https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/re2
/// </remarks>
internal class ExtractRegexFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);
        var pattern = (string?)arguments[0].Value;
        var captureGroup = (long?)arguments[1].Value;
        var value = (string?)arguments[2].Value;

        var regex = GetRegex(pattern);
        return new ScalarResult(ScalarTypes.String, GetResult(regex, captureGroup, value));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);
        Debug.Assert(
            arguments[0].Column.RowCount == arguments[1].Column.RowCount &&
            arguments[0].Column.RowCount == arguments[2].Column.RowCount);
        var patterns = (TypedBaseColumn<string?>)arguments[0].Column;
        var captureGroups = (TypedBaseColumn<long?>)arguments[1].Column;
        var values = (TypedBaseColumn<string?>)arguments[2].Column;

        var cacheEntry = (Pattern: (string?)null, Regex: (Regex?)null);
        var data = NullableSetBuilderOfstring.CreateFixed(values.RowCount);
        for (var i = 0; i < values.RowCount; i++)
        {
            var pattern = patterns[i];
            if (i == 0 || !string.Equals(pattern, cacheEntry.Pattern))
                cacheEntry = (Pattern: pattern, Regex: GetRegex(pattern));

            data[i] = GetResult(cacheEntry.Regex!, captureGroups[i], values[i]);
        }

        return new ColumnarResult(GenericColumnFactoryOfstring.CreateFromDataSet(data.ToNullableSet()));
    }

    private static Regex GetRegex(string? pattern) => new(pattern ?? string.Empty);

    private static string GetResult(Regex regex, long? captureGroup, string? input)
    {
        if (captureGroup is >= int.MinValue and <= int.MaxValue)
        {
            var captureGroupVal = (int)captureGroup.Value;
            var match = regex.Match(input ?? string.Empty);
            if (match.Success)
                if (captureGroupVal < match.Groups.Count)
                    return match.Groups[captureGroupVal].Value;
        }

        return string.Empty;
    }
}
