using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.HasAll",CustomContext = true)]
internal partial class HasAllOperator
{
    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        InOperatorCore.CheckString(StringComparison.InvariantCultureIgnoreCase, context, a, jsonArray,
            InOperatorCore.EqualsContains.Contains, InOperatorCore.AllAny.All);
}
