// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal interface IAggregateImpl
    {
        ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments);
    }
}
