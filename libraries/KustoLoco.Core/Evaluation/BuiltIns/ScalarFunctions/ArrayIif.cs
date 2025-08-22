using System;
using System.Text.Json.Nodes;
using System.Linq;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl
{
    [KustoImplementation(Keyword = "array_iif")]
    internal partial class ArrayIifFunction
    {
        public JsonNode? Impl(JsonNode condArray, JsonNode array1, JsonNode array2)
        {
            if (condArray is not JsonArray conds || array1 is not JsonArray arr1 || array2 is not JsonArray arr2)
                return null;
            var result = new JsonArray();

            for (int i = 0; i < conds.Count; i++)
            {
                bool? cond = null;
                if (conds[i] is JsonValue v)
                {
                    if (v.TryGetValue<bool>(out var b)) cond = b;
                    else if (v.TryGetValue<int>(out var e)) cond = e != 0;
                    else if (v.TryGetValue<long>(out var l)) cond = l != 0;
                    else if (v.TryGetValue<double>(out var d)) cond = d != 0.0;
                }

                JsonNode? val1;
                JsonNode? val2;

                if (arr1.Count == 1 && arr2.Count == 1)
                {
                    val1 = arr1[0];
                    val2 = arr2[0];
                }
                else
                {
                    val1 = i < arr1.Count ? arr1[i] : "null";
                    val2 = i < arr2.Count ? arr2[i] : "null";
                }

                if (cond is null)
                    result.Add(JsonValue.Create("null"));
                else
                {
                    var val = (cond.Value ? val1 : val2);
                    if (val is JsonValue jv)
                        result.Add(jv.GetValue<object>() ?? JsonValue.Create((object?)null));
                    else
                        result.Add(val ?? JsonValue.Create((object?)null));
                }
            }
            return result;
        } 
    }
}
