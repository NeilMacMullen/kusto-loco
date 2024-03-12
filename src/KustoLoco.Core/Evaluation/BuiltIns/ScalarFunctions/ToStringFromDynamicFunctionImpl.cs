using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class ToStringFromDynamicFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var dynamicValue = (JsonNode?)arguments[0].Value;
        return new ScalarResult(ScalarTypes.String, Impl(dynamicValue));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<JsonNode?>)arguments[0].Column;

        var data = new string?[column.RowCount];
        for (var i = 0; i < column.RowCount; i++)
        {
            data[i] = Impl(column[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? Impl(JsonNode? input)
    {
        if (input != null)
        {
            if (input is JsonValue valueNode)
            {
                var value = valueNode.GetValue<object>();
                if (value is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        return element.GetString();
                    }

                    if (element.ValueKind == JsonValueKind.Null)
                    {
                        return string.Empty;
                    }

                    // For any other value kind, continue below and use input.ToJsonString...
                }
                else if (value is string stringValue)
                {
                    return stringValue ?? string.Empty;
                }

                // For any other type, continue below and use input.ToJsonString...
            }

            return input.ToJsonString();
        }

        return string.Empty;
    }
}