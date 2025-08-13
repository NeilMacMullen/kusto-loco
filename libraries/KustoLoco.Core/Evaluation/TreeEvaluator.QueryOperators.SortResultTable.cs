//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.Util;
using IComparer = System.Collections.IComparer;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    internal class SortResultTable : ITableSource
    {
        private readonly EvaluationContext _context;
        private readonly TreeEvaluator _evaluator;
        private readonly ITableSource _input;
        private readonly (IRExpressionNode Expression, IComparer Comparer)[] _sortColumns;

        public SortResultTable(TreeEvaluator evaluator, EvaluationContext context, ITableSource input,
            (IRExpressionNode Expression, IComparer Comparer)[] sortColumns)
        {
            _input = input;
            _sortColumns = sortColumns;
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _context = context;
        }

        public TableSymbol Type => _input.Type;

        public IEnumerable<ITableChunk> GetData()
        {
            var allData = new List<object?>[_input.Type.Columns.Count];
            for (var i = 0; i < allData.Length; i++) allData[i] = [];

            var sortColumnsData = new List<object?>[_sortColumns.Length];
            for (var i = 0; i < _sortColumns.Length; i++) sortColumnsData[i] = [];

            foreach (var chunk in _input.GetData())
            {
                for (var i = 0; i < allData.Length; i++)
                for (var j = 0; j < chunk.RowCount; j++)
                    allData[i].Add(chunk.Columns[i].GetRawDataValue(j));

                var chunkContext = _context with { Chunk = chunk };
                for (var i = 0; i < _sortColumns.Length; i++)
                {
                    var sortExpression = _sortColumns[i].Expression;
                    var sortExpressionResult = sortExpression.Accept(_evaluator, chunkContext);
                    MyDebug.Assert(sortExpressionResult != EvaluationResult.Null);
                    var sortedChunkColumn = ((ColumnarResult)sortExpressionResult).Column;
                    for (var j = 0; j < sortedChunkColumn.RowCount; j++)
                        sortColumnsData[i].Add(sortedChunkColumn.GetRawDataValue(j));
                }
            }

            var sortedIndexes = new int[sortColumnsData[0].Count];
            for (var i = 0; i < sortedIndexes.Length; i++) sortedIndexes[i] = i;

            Array.Sort(
                sortedIndexes,
                (a, b) =>
                {
                    for (var i = 0; i < _sortColumns.Length; i++)
                    {
                        var column = sortColumnsData[i];
                        var result = _sortColumns[i].Comparer.Compare(column[a], column[b]);

                        if (result != 0) return result;
                    }

                    return 0;
                });

            var resultColumns = new BaseColumnBuilder[allData.Length];
            for (var i = 0; i < resultColumns.Length; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(_input.Type.Columns[i].Type);
                foreach (var t in sortedIndexes)
                    resultColumns[i].Add(allData[i][t]);
            }

            var resultChunk = new TableChunk(this, resultColumns.Select(c => c.ToColumn()).ToArray());
            yield return resultChunk;
        }
    }
}
