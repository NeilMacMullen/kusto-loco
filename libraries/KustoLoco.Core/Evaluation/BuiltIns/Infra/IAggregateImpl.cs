// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal interface IAggregateImpl
{
    ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments);
}