using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.InCs",CustomContext = true)]
internal partial class InCsOperator
{
    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        InOperatorCore.CheckString(StringComparison.InvariantCultureIgnoreCase, context, a, jsonArray,
            InOperatorCore.EqualsContains.Equals, InOperatorCore.AllAny.Any);
}
