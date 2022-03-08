// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitMaterializeExpression(IRMaterializeExpressionNode node, EvaluationContext context)
        {
            Debug.Assert(context.Left == null);

            var innerResult = (TabularResult?)node.Expression.Accept(this, context);
            Debug.Assert(innerResult != null);

            var result = new MaterializedTableResult(innerResult.Value);
            return new TabularResult(result);
        }

        private class MaterializedTableResult : ITableSource
        {
            private readonly ITableSource _input;
            private List<TableChunk>? _chunks;
            private int _hydrated;

            public MaterializedTableResult(ITableSource input)
            {
                _input = input ?? throw new ArgumentNullException(nameof(input));
            }

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

            public async IAsyncEnumerable<ITableChunk> GetDataAsync([EnumeratorCancellation]CancellationToken cancellation = default)
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
                    throw new InvalidOperationException("Attempted to hydrate materialized results more than once, possible race condition detected in consumption patterns.");
                }
            }
        }
    }
}
