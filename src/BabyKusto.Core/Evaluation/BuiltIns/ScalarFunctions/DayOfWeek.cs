// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class DayOfWeekFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var date = (DateTime?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.TimeSpan, Impl(date));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var dates = (Column<DateTime?>)arguments[0].Column;

            var data = new TimeSpan?[dates.RowCount];
            for (int i = 0; i < dates.RowCount; i++)
            {
                data[i] = Impl(dates[i]);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.TimeSpan, data));
        }

        private static TimeSpan? Impl(DateTime? input)
        {
            if (input.HasValue)
            {
                // Sunday: 0, Monday: 1, etc...
                // Luckily, this matches how enum DayOfWeek is defined
                return TimeSpan.FromDays((int)input.Value.DayOfWeek);
            }

            return null;
        }
    }
}
