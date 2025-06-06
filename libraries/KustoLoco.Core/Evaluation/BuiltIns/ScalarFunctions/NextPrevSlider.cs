﻿using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal static class NextPrevSlider
{
    internal static ColumnarResult InvokeColumnar<T>(ColumnarResult[] arguments,bool negatePad)
    {
        var column = (TypedBaseColumn<T?>)arguments[0].Column;
        if (column.RowCount == 0)
            return new ColumnarResult(column.Slice(0, 0)); // Return an empty column of the expected type

        var pad = 1L;
        if (arguments.Length > 1)
        {
            var offsetColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            pad = (offsetColumn.GetRawDataValue(0) as long?) ?? 1; // Default to 1 if no offset is provided
        }
        object? defaultValue = null;
        if (arguments.Length > 2)
        {
            var defaultColumn = arguments[2].Column;
            defaultValue = defaultColumn.GetRawDataValue(0);
        }
        if (negatePad) pad = -pad;
        var data = column.Slide((int)pad, defaultValue);
        return new ColumnarResult(data);
    }
}
