// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class MakeListIntFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
            var valuesColumn = (Column<int?>)arguments[0].Column;

            long maxSize = long.MaxValue;
            if (arguments.Length == 2)
            {
                var maxSizeColumn = (Column<long?>)arguments[1].Column;
                Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

                if (maxSizeColumn.RowCount > 0)
                {
                    maxSize = maxSizeColumn[0] ?? long.MaxValue;
                }
            }

            var list = new List<int?>();
            for (int i = 0; i < valuesColumn.RowCount; i++)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    list.Add(v);
                    if (list.Count >= maxSize)
                    {
                        break;
                    }
                }
            }

            return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
        }
    }

    internal class MakeListLongFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
            var valuesColumn = (Column<long?>)arguments[0].Column;

            long maxSize = long.MaxValue;
            if (arguments.Length == 2)
            {
                var maxSizeColumn = (Column<long?>)arguments[1].Column;
                Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

                if (maxSizeColumn.RowCount > 0)
                {
                    maxSize = maxSizeColumn[0] ?? long.MaxValue;
                }
            }

            var list = new List<long?>();
            for (int i = 0; i < valuesColumn.RowCount; i++)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    list.Add(v);
                    if (list.Count >= maxSize)
                    {
                        break;
                    }
                }
            }

            return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
        }
    }

    internal class MakeListDoubleFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
            var valuesColumn = (Column<double?>)arguments[0].Column;

            long maxSize = long.MaxValue;
            if (arguments.Length == 2)
            {
                var maxSizeColumn = (Column<long?>)arguments[1].Column;
                Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

                if (maxSizeColumn.RowCount > 0)
                {
                    maxSize = maxSizeColumn[0] ?? long.MaxValue;
                }
            }

            var list = new List<double?>();
            for (int i = 0; i < valuesColumn.RowCount; i++)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    list.Add(v);
                    if (list.Count >= maxSize)
                    {
                        break;
                    }
                }
            }

            return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
        }
    }

    internal class MakeListStringFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
            var valuesColumn = (Column<string?>)arguments[0].Column;

            long maxSize = long.MaxValue;
            if (arguments.Length == 2)
            {
                var maxSizeColumn = (Column<long?>)arguments[1].Column;
                Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

                if (maxSizeColumn.RowCount > 0)
                {
                    maxSize = maxSizeColumn[0] ?? long.MaxValue;
                }
            }

            var list = new List<string?>();
            for (int i = 0; i < valuesColumn.RowCount; i++)
            {
                var v = valuesColumn[i];
                if (!string.IsNullOrEmpty(v))
                {
                    list.Add(v);
                    if (list.Count >= maxSize)
                    {
                        break;
                    }
                }
            }

            return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
        }
    }
}
