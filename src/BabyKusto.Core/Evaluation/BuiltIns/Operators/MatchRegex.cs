// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    /// <remarks>
    /// This uses the built-in .NET Regex implementation, which isn't strictly compatible with Kusto's RE2.
    /// See: https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/re2
    /// </remarks>
    internal class MatchRegexOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var value = (string?)arguments[0].Value;
            var pattern = (string?)arguments[1].Value;

            var regex = GetRegex(pattern);
            return new ScalarResult(ScalarTypes.Bool, GetResult(regex, value));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var values = (Column<string?>)(arguments[0].Column);
            var patterns = (Column<string?>)(arguments[1].Column);

            var cacheEntry = (Pattern: (string?)null, Regex: (Regex?)null);
            var data = new bool?[values.RowCount];
            for (int i = 0; i < values.RowCount; i++)
            {
                var pattern = patterns[i];
                if (i == 0 || !string.Equals(pattern, cacheEntry.Pattern))
                {
                    cacheEntry = (Pattern: pattern, Regex: GetRegex(pattern));
                }

                data[i] = GetResult(cacheEntry.Regex!, values[i]);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }

        private static Regex GetRegex(string? pattern)
        {
            return new Regex(pattern ?? string.Empty);
        }

        private static bool GetResult(Regex regex, string? input)
        {
            return regex.IsMatch(input ?? string.Empty);
        }
    }
}
