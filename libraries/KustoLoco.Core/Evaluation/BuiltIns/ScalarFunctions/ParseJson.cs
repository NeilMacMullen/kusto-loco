// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class ParseJsonStringFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var text = (string?)arguments[0].Value;
        return new ScalarResult(ScalarTypes.Dynamic, ParseInternal(text));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<string?>)arguments[0].Column;

        var data = new JsonNode?[column.RowCount];
        for (var i = 0; i < column.RowCount; i++)
        {
            data[i] = ParseInternal(column[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    private static JsonNode? ParseInternal(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(input);
        }
        catch (JsonException)
        {
            // From official docs at https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/parsejsonfunction:
            // If json is of type string, but it isn't a properly formatted JSON string, then the returned value is an object of type dynamic that holds the original string value.
            return JsonValue.Create(input);
        }
    }
}

/// <remarks>
///     From docs at <see href="https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/parsejsonfunction" />:
///     If json is of type dynamic, its value is used as-is.
/// </remarks>
internal class ParseJsonDynamicFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        Debug.Assert(arguments[0].Value is null or JsonNode);
        return arguments[0];
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<JsonNode?>)arguments[0].Column;

        return new ColumnarResult(column);
    }
}