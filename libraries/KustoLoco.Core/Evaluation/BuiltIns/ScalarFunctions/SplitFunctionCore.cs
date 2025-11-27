using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class SplitFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2);

        return new ScalarResult(ScalarTypes.DynamicArrayOfString,
            Evaluate(arguments[0].Value as string, arguments[1].Value as string));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2);

        var columns = new GenericTypedBaseColumnOfstring[2];
        for (var i = 0; i < arguments.Length; i++)
        {
            columns[i] = (GenericTypedBaseColumnOfstring)arguments[i].Column;
        }

        var rowCount = columns[0].RowCount;
        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);
        for (var i = 0; i < rowCount; i++)
        {
            data[i] = Evaluate(columns[0][i], columns[1][i]);
        }

        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }

    protected JsonArray? Evaluate(string? source, string? delimiter)
    {
        if (source is null) return null;
        if (delimiter is null) return null;

        var toks = source.Split(delimiter);
        var n = toks.Select(t => JsonSerializer.SerializeToNode(t)).ToArray();
        return new JsonArray(n);
    }
}

internal class SplitWithIndexFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 3);

        var result = Evaluate(arguments[0].Value as string, arguments[1].Value as string, GetLongValue(arguments[2].Value));
        // Return as Dynamic type with the string value serialized as a JsonNode
        var jsonValue = result != null ? JsonValue.Create(result) : null;
        return new ScalarResult(ScalarTypes.Dynamic, jsonValue);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 3);

        var sourceColumn = (GenericTypedBaseColumnOfstring)arguments[0].Column;
        var delimiterColumn = (GenericTypedBaseColumnOfstring)arguments[1].Column;
        var indexColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;

        var rowCount = sourceColumn.RowCount;
        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);
        for (var i = 0; i < rowCount; i++)
        {
            var result = Evaluate(sourceColumn[i], delimiterColumn[i], indexColumn[i]);
            data[i] = result != null ? JsonValue.Create(result) : null;
        }

        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }

    private static long? GetLongValue(object? value)
    {
        return value switch
        {
            long l => l,
            int i => i,
            JsonValue jv when jv.TryGetValue<long>(out var lv) => lv,
            JsonValue jv when jv.TryGetValue<int>(out var iv) => iv,
            _ => null
        };
    }

    private static string? Evaluate(string? source, string? delimiter, long? requestedIndex)
    {
        if (source is null) return null;
        if (delimiter is null) return null;
        if (requestedIndex is null) return null;

        var toks = source.Split(delimiter);
        var indexValue = requestedIndex.Value;

        // Handle overflow - if index is too large/small, it will be out of range anyway
        if (indexValue > int.MaxValue || indexValue < int.MinValue)
        {
            return string.Empty;
        }

        var index = (int)indexValue;

        // Handle negative indices (from the end of the array)
        if (index < 0)
        {
            index = toks.Length + index;
        }

        // Return empty string if index is out of range
        if (index < 0 || index >= toks.Length)
        {
            return string.Empty;
        }

        return toks[index];
    }
}
