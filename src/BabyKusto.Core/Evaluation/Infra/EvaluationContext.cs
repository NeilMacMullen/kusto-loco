// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core.Evaluation
{
    internal record EvaluationContext(LocalScope Scope, TabularResult? Left = null, ITableChunk? Chunk = null);
}
