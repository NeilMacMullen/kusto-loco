using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using NotNullStrings;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToString")]
internal partial class ToStringFunction
{
    private static string DecImpl(decimal input) => input.ToString(CultureInfo.InvariantCulture);
    private static string IntImpl(int input) => input.ToString(CultureInfo.InvariantCulture);
    private static string LongImpl(long input) => input.ToString(CultureInfo.InvariantCulture);
    private static string DoubleImpl(double input) => input.ToString(CultureInfo.InvariantCulture);
    private static string StringImpl(string input) => input;
    private static string GuidImpl(string input) => input.ToString(CultureInfo.InvariantCulture);
    private static string DtImpl(DateTime input) => input.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
    private static string TsImpl(TimeSpan input) => input.ToString();

    private static string DynImpl(JsonNode input)
    {
        if (input is JsonValue valueNode)
        {
            var value = valueNode.GetValue<object>();
            if (value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.String) return element.GetString().NullToEmpty();

                if (element.ValueKind == JsonValueKind.Null) return string.Empty;

                // For any other value kind, continue below and use input.ToJsonString...
            }
            else if (value is string stringValue)
            {
                return stringValue;
            }

            // For any other type, continue below and use input.ToJsonString...
        }

        return input.ToJsonString();
    }
}
