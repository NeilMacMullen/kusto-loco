// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class EndOfWeekFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var date = (DateTime?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.DateTime, Impl(date));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var dates = (Column<DateTime?>)arguments[0].Column;

            var data = new DateTime?[dates.RowCount];
            for (int i = 0; i < dates.RowCount; i++)
            {
                data[i] = Impl(dates[i]);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }

        private static DateTime? Impl(DateTime? input)
        {
            if (input.HasValue)
            {
                var startOfDay = new DateTime(
                    year: input.Value.Year,
                    month: input.Value.Month,
                    day: input.Value.Day,
                    hour: 0,
                    minute: 0,
                    second: 0,
                    kind: input.Value.Kind);
                var startOfWeek = startOfDay.AddDays(-(int)startOfDay.DayOfWeek);
                return startOfWeek.AddDays(7).AddTicks(-1);
            }

            return null;
        }
    }
}
