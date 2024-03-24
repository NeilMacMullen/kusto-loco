// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation;

internal readonly record struct EvaluationContext
{
    public readonly LocalScope Scope;

    public EvaluationContext(LocalScope scope, TabularResult left, ITableChunk chunk)
    {
        Scope = scope;
        Left = left;
        Chunk = chunk;
    }

    public EvaluationContext(LocalScope scope)
        : this(scope, TabularResult.Empty, TableChunk.Empty)
    {
    }

    public EvaluationContext(LocalScope scope, ITableChunk chunk)
        : this(scope, TabularResult.Empty, chunk)
    {
    }

    public ITableChunk Chunk { get; init; }

    public TabularResult Left { get; init; }
}