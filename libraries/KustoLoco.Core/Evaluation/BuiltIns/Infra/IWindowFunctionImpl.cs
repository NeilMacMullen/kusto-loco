// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal interface IWindowFunctionImpl
{
    ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult);
}