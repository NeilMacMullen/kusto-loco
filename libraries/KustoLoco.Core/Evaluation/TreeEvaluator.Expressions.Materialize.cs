// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using KustoLoco.Core.Extensions;
using KustoLoco.Core.InternalRepresentation;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitMaterializeExpression(IRMaterializeExpressionNode node,
        EvaluationContext context)
    {
        Debug.Assert(context.Left == TabularResult.Empty);

        var innerResult = (TabularResult)node.Expression.Accept(this, context);
        Debug.Assert(innerResult != TabularResult.Empty);

        var result = new MaterializedTableResult(innerResult.Value);
        return TabularResult.CreateWithVisualisation(result, innerResult.VisualizationState);
    }

    private class MaterializedTableResult : ITableSource
    {
        private readonly ITableSource _input;
        private List<TableChunk>? _chunks;
        private int _hydrated;

        public MaterializedTableResult(ITableSource input) =>
            _input = input;

        public TableSymbol Type => _input.Type;

        public IEnumerable<ITableChunk> GetData()
        {
            if (_chunks == null)
            {
                EnsureSingleHydration();

                _chunks = new List<TableChunk>();
                foreach (var chunk in _input.GetData())
                {
                    _chunks.Add(chunk.ReParent(this));
                }
            }

            return _chunks.AsReadOnly();
        }

        public async IAsyncEnumerable<ITableChunk> GetDataAsync(
            [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            if (_chunks == null)
            {
                EnsureSingleHydration();

                _chunks = new List<TableChunk>();
                await foreach (var chunk in _input.GetDataAsync(cancellation))
                {
                    _chunks.Add(chunk.ReParent(this));
                }
            }

            foreach (var chunk in _chunks)
            {
                yield return chunk;
            }
        }

        private void EnsureSingleHydration()
        {
            if (Interlocked.CompareExchange(ref _hydrated, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "Attempted to hydrate materialized results more than once, possible race condition detected in consumption patterns.");
            }
        }
    }
}