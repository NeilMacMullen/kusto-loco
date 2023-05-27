// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class MakeListWithNullsIntFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var valuesColumn = (Column<int?>)arguments[0].Column;

            var list = new List<int?>();
            for (int i = 0; i < valuesColumn.RowCount; i++)
            {
                list.Add(valuesColumn[i]);
            }

            return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
        }
    }

    internal class MakeListWithNullsLongFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var valuesColumn = (Column<long?>)arguments[0].Column;

            var list = new List<long?>();
            for (int i = 0; i < valuesColumn.RowCount; i++)
            {
                list.Add(valuesColumn[i]);
            }

            return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
        }
    }

    internal class MakeListWithNullsDoubleFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var valuesColumn = (Column<double?>)arguments[0].Column;

            var list = new List<double?>();
            for (int i = 0; i < valuesColumn.RowCount; i++)
            {
                list.Add(valuesColumn[i]);
            }

            return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
        }
    }

    internal class MakeListWithNullsStringFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var valuesColumn = (Column<string?>)arguments[0].Column;

            var list = new List<string?>();
            for (int i = 0; i < valuesColumn.RowCount; i++)
            {
                list.Add(valuesColumn[i]);
            }

            return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
        }
    }
}
