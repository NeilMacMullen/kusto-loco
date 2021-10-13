using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;
using KustoExecutionEngine.Core.Expressions;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingSummarizeOperator : StirlingOperator<SummarizeOperator>
    {
        private readonly List<string> _byExpressions;
        private readonly List<(string Name, StirlingExpression)> _aggregationExpressions;

        public StirlingSummarizeOperator(StirlingEngine engine, SummarizeOperator expression)
            : base(engine, expression)
        {
            _aggregationExpressions =
                expression.Aggregates.Select((s, i) =>
                    (expression.ResultType.Members[i].Name, StirlingExpression.Build(engine, s.Element))).ToList();
            _byExpressions = expression.ByClause.Expressions
                .Select(s => s.Element).Cast<NameReference>().Select(s => s.SimpleName)
                .ToList();
        }

        protected override ITabularSourceV2 EvaluateInternal(ITabularSourceV2 input)
        {
            /*
            var dictionary = new Dictionary<int, List<IRow>>();
            IRow? nextRow = input.GetNextRow();
            while (nextRow != null)
            {
                var aggKeyList = new List<object?>();
                foreach (string columnName in _byExpressions)
                {
                    aggKeyList.Add(nextRow[columnName]);
                }

                int listHashCode = GetListHashCode(aggKeyList);
                if (!dictionary.TryGetValue(listHashCode, out var rows))
                {
                    rows = new List<IRow>();
                    dictionary[listHashCode] = rows;
                }

                rows.Add(nextRow);
                nextRow = input.GetNextRow();
            }

            var summarizedTable = new List<IRow>();
            foreach ((int _, var rows) in dictionary)
            {
                var row = new Dictionary<string, object?>();
                foreach (var (columnName, aggregationExpression) in _aggregationExpressions)
                {
                    row[columnName] = aggregationExpression.Evaluate(rows);
                }

                summarizedTable.Add(new Row(row));
            }

            return new InMemoryTabularSource(summarizedTable.ToArray());
            */
            return new EmptyTabularSourceV2();
        }

        private static int GetListHashCode(IEnumerable<object?> objects)
        {
            return objects.Aggregate(0, (x, y) => x.GetHashCode() ^ y?.GetHashCode() ?? 0);
        }
    }
}