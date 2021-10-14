using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Syntax;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core.Expressions.Operators
{
    internal sealed class StirlingSummarizeOperator : StirlingOperator<SummarizeOperator>
    {
        private readonly List<(string Name, StirlingExpression Expression)> _byExpressions;
        private readonly List<(string Name, StirlingExpression Expression)> _aggregationExpressions;

        public StirlingSummarizeOperator(StirlingEngine engine, SummarizeOperator expression)
            : base(engine, expression)
        {
            _aggregationExpressions =
                expression.Aggregates.Select((s, i) =>
                    (expression.ResultType.Members[i].Name, StirlingExpression.Build(engine, s.Element))).ToList();
            _byExpressions = expression.ByClause.Expressions.Select((s, i) =>
                    (expression.ResultType.Members[i].Name, StirlingExpression.Build(engine, s.Element))).ToList();
        }

        protected override ITabularSourceV2 EvaluateTableInputInternal(ITabularSourceV2 input)
        {
            return new SummarizeResultTable(_engine, input, _byExpressions, _aggregationExpressions);
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
        }

        private static int GetListHashCode(IEnumerable<object?> objects)
        {
            return objects.Aggregate(0, (x, y) => x.GetHashCode() ^ y?.GetHashCode() ?? 0);
        }

        internal class SummarizeResultTable : ITabularSourceV2
        {
            private readonly StirlingEngine _engine;
            private readonly ITabularSourceV2 _input;
            private readonly List<(string Name, StirlingExpression Expression)> _byExpressions;
            private readonly List<(string Name, StirlingExpression Expression)> _aggregationExpressions;

            public SummarizeResultTable(
                StirlingEngine engine,
                ITabularSourceV2 input,
                List<(string Name, StirlingExpression Expression)> byExpressions,
                List<(string Name, StirlingExpression Expression)> aggregationExpressions)
            {
                _engine = engine;
                _input = input;
                _byExpressions = byExpressions;
                _aggregationExpressions = aggregationExpressions;
            }

            public TableSchema Schema => TableSchema.Empty; // TODO

            public IEnumerable<ITableChunk> GetData()
            {
                // TODO: Implement summarize
                // Open questions:
                //   * How to avoid hydrating data from _input more than once
                //   * How to bucket rows from `_input` as they come into each summarize bucket.


                var bucketizedTables = new Dictionary<string, List<object?>[]>();
                // For now, just pass-through the source. Oops..
                foreach (var chunk in _input.GetData())
                {
                    for (int i = 0; i < chunk.RowCount; i++)
                    {
                        var row = chunk.GetRow(i);

                        var byValues = _byExpressions.Select(e => e.Expression.Evaluate(row)).ToList();

                        // TODO: Should nulls be treated differently than empty string?
                        // TODO: Use a less expensive composite key computation
                        var key = string.Join("|", byValues.Select(v => Uri.EscapeDataString(v?.ToString() ?? "")));
                        if (!bucketizedTables.TryGetValue(key, out var bucket))
                        {
                            bucketizedTables[key] = bucket = new List<object?>[_input.Schema.ColumnDefinitions.Count];
                            for (int j = 0; j < _input.Schema.ColumnDefinitions.Count; j++)
                            {
                                bucket[j] = new List<object?>();
                            }
                        }

                        for (int j = 0; j < bucket.Length; j++)
                        {
                            bucket[j].Add(row.Values[j]);
                        }
                    }
                    yield return chunk;
                }

                yield break;
            }
        }
    }
}