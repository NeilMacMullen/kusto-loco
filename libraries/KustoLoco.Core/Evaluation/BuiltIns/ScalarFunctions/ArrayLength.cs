// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;


[KustoImplementation(Keyword = "Functions.ArrayLength")]
public partial class ArrayLengthFunction
{
    public long? Impl(JsonNode node)
    {
        var a = node as JsonArray;
        return a?.Count ?? null;
    }
}
