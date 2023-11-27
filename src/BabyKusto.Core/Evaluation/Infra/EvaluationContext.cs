// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core.Evaluation;

internal readonly record struct EvaluationContext
{
    public readonly LocalScope Scope;

    public EvaluationContext(LocalScope scope, TabularResult? left, ITableChunk chunk)
    {
        Scope = scope;
        Left = left;
        Chunk = chunk;
    }

    public EvaluationContext(LocalScope scope)
        : this(scope, null, TableChunk.Empty)
    {
    }

    public EvaluationContext(LocalScope scope, ITableChunk chunk)
        : this(scope, null, chunk)
    {
    }

    public ITableChunk Chunk { get; init; }

    public TabularResult? Left { get; init; }
}