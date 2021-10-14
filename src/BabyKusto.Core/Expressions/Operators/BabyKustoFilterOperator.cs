// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions.Operators
{
    internal sealed class BabyKustoFilterOperator : BabyKustoOperator<FilterOperator>
    {
        private readonly BabyKustoExpression _conditionExpression;

        public BabyKustoFilterOperator(BabyKustoEngine engine, FilterOperator expression)
            : base(engine, expression)
        {
            _conditionExpression = BabyKustoExpression.Build(engine, expression.Condition);
        }

        protected override ITableSource EvaluateTableInputInternal(ITableSource input)
        {
            return new DerivedTableSource(
                input,
                input.Schema,
                chunk =>
                {
                    int numColumns = input.Schema.ColumnDefinitions.Count;
                    var resultsData = new List<object?>[numColumns];
                    for (int i = 0; i < numColumns; i++)
                    {
                        resultsData[i] = new List<object?>();
                    }

                    for (int i = 0; i < chunk.RowCount; i++)
                    {
                        var row = chunk.GetRow(i);

                        // TODO: Should we throw if `_conditionExpression` evaluates to a different data type?
                        if (_conditionExpression.Evaluate(row) is bool boolResult && boolResult)
                        {
                            for (int j = 0; j < numColumns; j++)
                            {
                                resultsData[j].Add(row.Values[j]);
                            }
                        }
                    }

                    return new TableChunk(input.Schema, resultsData.Select(c => new Column(c.ToArray())).ToArray());
                });
        }
    }
}
