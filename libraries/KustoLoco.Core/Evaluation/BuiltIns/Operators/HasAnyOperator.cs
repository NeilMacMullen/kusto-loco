using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.HasAny",CustomContext = true)]
internal partial class HasAnyOperator
{
    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        InOperatorCore.CheckString(StringComparison.InvariantCultureIgnoreCase, context, a, jsonArray,
            InOperatorCore.EqualsContains.Contains, InOperatorCore.AllAny.Any);
}
