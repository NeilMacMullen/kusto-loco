using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.NotIn",CustomContext = true)]
internal partial class NotInOperator
{
    private static bool LongImpl(InLongContext context, long a, JsonNode jsonArray) =>
        !InOperatorCore.CheckLong(context, a, jsonArray);

    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        !InOperatorCore.CheckString(StringComparison.InvariantCulture, context, a, jsonArray,
            InOperatorCore.EqualsContains.Equals, InOperatorCore.AllAny.Any);
}
