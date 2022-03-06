// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal interface IWindowFunctionImpl
    {
        ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments, ColumnarResult? previousWindowResult);
    }
}
