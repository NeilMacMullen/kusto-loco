// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions.Operators
{
    internal sealed class BabyKustoSummarizeOperator : BabyKustoOperator<SummarizeOperator>
    {
        private readonly List<BabyKustoExpression> _byExpressions;
        private readonly List<BabyKustoExpression> _aggregationExpressions;
        private readonly TableSchema _resultSchema;

        public BabyKustoSummarizeOperator(BabyKustoEngine engine, SummarizeOperator expression)
            : base(engine, expression)
        {
            _aggregationExpressions =
                expression.Aggregates.Select((s, i) => BabyKustoExpression.Build(engine, s.Element)).ToList();
            _byExpressions = expression.ByClause is null
                ? new List<BabyKustoExpression>()
                : expression.ByClause.Expressions.Select((s, i) => BabyKustoExpression.Build(engine, s.Element)).ToList();

            _resultSchema = new TableSchema(
                expression.ResultType.Members.Select(m => new ColumnDefinition(m.Name, KustoValueKind.Real)).ToList());
        }

        protected override ITableSource EvaluateTableInputInternal(ITableSource input)
        {
            return new SummarizeResultTable(input, _byExpressions, _aggregationExpressions, _resultSchema);
        }

        internal class SummarizeResultTable : ITableSource
        {
            private readonly ITableSource _input;
            private readonly List<BabyKustoExpression> _byExpressions;
            private readonly List<BabyKustoExpression> _aggregationExpressions;
            private readonly TableSchema _resultSchema;

            public SummarizeResultTable(
                ITableSource input,
                List<BabyKustoExpression> byExpressions,
                List<BabyKustoExpression> aggregationExpressions,
                TableSchema resultSchema)
            {
                _input = input;
                _byExpressions = byExpressions;
                _aggregationExpressions = aggregationExpressions;
                _resultSchema = resultSchema;
            }

            public TableSchema Schema => _resultSchema;

            public IEnumerable<ITableChunk> GetData()
            {
                // TODO: This is horribly inefficient
                //  * Copies all data, even columns that aren't used
                //  * Composite key calculation involves lots of string allocations and escapings

                var bucketizedTables = new Dictionary<string, (List<object?> ByValues, List<object?>[] OriginalData)>();
                int numInputColumns = _input.Schema.ColumnDefinitions.Count;
                foreach (var chunk in _input.GetData())
                {
                    for (int i = 0; i < chunk.RowCount; i++)
                    {
                        var row = chunk.GetRow(i);

                        var byValues = _byExpressions.Select(e => e.Evaluate(row)).ToList();

                        // TODO: Should nulls be treated differently than empty string?
                        // TODO: Use a less expensive composite key computation
                        var key = string.Join("|", byValues.Select(v => Uri.EscapeDataString(v?.ToString() ?? "")));
                        if (!bucketizedTables.TryGetValue(key, out var bucket))
                        {
                            bucketizedTables[key] = bucket = (byValues, new List<object?>[numInputColumns]);
                            for (int j = 0; j < numInputColumns; j++)
                            {
                                bucket.OriginalData[j] = new List<object?>();
                            }
                        }

                        for (int j = 0; j < numInputColumns; j++)
                        {
                            bucket.OriginalData[j].Add(row.Values[j]);
                        }
                    }
                }

                var resultsData = new object?[_byExpressions.Count + _aggregationExpressions.Count][];
                for (int i = 0; i < resultsData.Length; i++)
                {
                    resultsData[i] = new object?[bucketizedTables.Count];
                }

                int resultRow = 0;
                foreach (var tableData in bucketizedTables.Values)
                {
                    for (int i = 0; i < tableData.ByValues.Count; i++)
                    {
                        resultsData[i][resultRow] = tableData.ByValues[i];
                    }

                    var tableForAggregation = new InMemoryTableSource(_input.Schema, tableData.OriginalData.Select(c => new Column(c.ToArray())).ToArray());

                    for (int i = 0; i < _aggregationExpressions.Count; i++)
                    {
                        var aggregationExpression = _aggregationExpressions[i];
                        var aggregation = aggregationExpression.Evaluate(tableForAggregation);
                        resultsData[tableData.ByValues.Count + i][resultRow] = aggregation;
                    }

                    resultRow++;
                }

                var resultChunk = new TableChunk(_resultSchema, resultsData.Select(c => new Column(c)).ToArray());
                yield return resultChunk;
            }
        }
    }
}