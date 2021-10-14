using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Syntax;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingFilterOperator : StirlingOperator<FilterOperator>
    {
        private readonly StirlingExpression conditionExpression;

        public StirlingFilterOperator(StirlingEngine engine, FilterOperator expression)
            : base(engine, expression)
        {
            this.conditionExpression = StirlingExpression.Build(engine, expression.Condition);
        }

        protected override ITableSource EvaluateTableInputInternal(ITableSource input)
        {
            return new DerivedTableSource(
                input,
                input.Schema,
                chunk =>
                {
                    var filterResult = new List<IRow?>();
                    for(int i = 0; i < chunk.RowCount; i++)
                    {
                        var row = chunk.GetRow(i);
                        if ((bool)this.conditionExpression.Evaluate(row))
                        {
                            filterResult.Add(row);
                        }
                    }

                    var resultsData = new object?[input.Schema.ColumnDefinitions.Count][];
                    for (int i = 0; i < resultsData.Length; i++)
                    {
                        resultsData[i] = new object?[filterResult.Count];
                    }

                    for (int i = 0; i < filterResult.Count; i++)
                    {
                        for (int j = 0; j < filterResult[i].Values.Length; j++)
                        {
                            resultsData[j][i] = filterResult[i].Values[j];
                        }
                    }

                    return new TableChunk(input.Schema, resultsData.Select(c => new Column(c)).ToArray());
                });
        }
    }
}
