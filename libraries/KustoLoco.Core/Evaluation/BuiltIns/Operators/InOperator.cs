using System;
using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

public static class InOperatorCore
{
    public enum AllAny
    {
        All,
        Any
    }

    public enum EqualsContains
    {
        Equals,
        Contains
    }

    public static bool CheckString(StringComparison comparison, InContext context, string a, JsonNode jsonArray,
        EqualsContains equals,
        AllAny allAny)
    {
        if (context.LastA == a &&
            context.LastNode == jsonArray)
            return context.LastResult;

        if (jsonArray is JsonArray array)
        {
            context.LastA = a;
            //check the context so that we can avoid repeated work
            if (context.LastNode != jsonArray)
            {
                var stringArray = array.Select(n => n!.ToString())
                    .ToArray();

                context.LastNode = jsonArray;
                context.LastStringArray = stringArray;
            }

            context.LastResult =
                allAny == AllAny.Any
                    ? equals == EqualsContains.Equals
                        ? context.LastStringArray.Any(x => a.Equals(x, comparison))
                        : context.LastStringArray.Any(x => a.Contains(x, comparison))
                    : equals == EqualsContains.Equals
                        ? context.LastStringArray.All(x => a.Equals(x, comparison))
                        : context.LastStringArray.All(x => a.Contains(x, comparison))
                ;
            return context.LastResult;
        }

        return false;
    }

    public static bool CheckLong(InLongContext context, long a, JsonNode jsonArray)
    {
        if (context.LastA == a &&
            context.LastNode == jsonArray)
            return context.LastResult;

        if (jsonArray is JsonArray array)
        {
            context.LastA = a;
            //check the context so that we can avoid repeated work
            if (context.LastNode != jsonArray)
            {
                var longs = array.Select(n => n!.GetValue<long?>())
                    .ToArray();

                context.LastNode = jsonArray;
                context.LastLongArray = longs;
            }

            context.LastResult = context.LastLongArray.Any(x => a.Equals(x));
            return context.LastResult;
        }

        return false;
    }
}

[KustoImplementation(Keyword = "Operators.In")]
internal partial class InOperator
{
    private static bool LongImpl(InLongContext context, long a, JsonNode jsonArray) =>
        InOperatorCore.CheckLong(context, a, jsonArray);

    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        InOperatorCore.CheckString(StringComparison.InvariantCulture, context, a, jsonArray,
            InOperatorCore.EqualsContains.Equals,InOperatorCore.AllAny.Any);
}


[KustoImplementation(Keyword = "Operators.NotIn")]
internal partial class NotInOperator
{
    private static bool LongImpl(InLongContext context, long a, JsonNode jsonArray) =>
        !InOperatorCore.CheckLong(context, a, jsonArray);

    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        !InOperatorCore.CheckString(StringComparison.InvariantCulture, context, a, jsonArray,
            InOperatorCore.EqualsContains.Equals, InOperatorCore.AllAny.Any);
}

[KustoImplementation(Keyword = "Operators.InCs")]
internal partial class InCsOperator
{
    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        InOperatorCore.CheckString(StringComparison.InvariantCultureIgnoreCase, context, a, jsonArray,
            InOperatorCore.EqualsContains.Equals, InOperatorCore.AllAny.Any);
}


[KustoImplementation(Keyword = "Operators.NotInCs")]
internal partial class NotInCsOperator
{
    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
       ! InOperatorCore.CheckString(StringComparison.InvariantCultureIgnoreCase, context, a, jsonArray,
            InOperatorCore.EqualsContains.Equals, InOperatorCore.AllAny.Any);
}

[KustoImplementation(Keyword = "Operators.HasAny")]
internal partial class HasAnyOperator
{
    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        InOperatorCore.CheckString(StringComparison.InvariantCultureIgnoreCase, context, a, jsonArray,
            InOperatorCore.EqualsContains.Contains, InOperatorCore.AllAny.Any);
}

[KustoImplementation(Keyword = "Operators.HasAll")]
internal partial class HasAllOperator
{
    private static bool Impl(InContext context, string a, JsonNode jsonArray) =>
        InOperatorCore.CheckString(StringComparison.InvariantCultureIgnoreCase, context, a, jsonArray,
            InOperatorCore.EqualsContains.Contains, InOperatorCore.AllAny.All);
}

public class InContext
{
    public string LastA = string.Empty;
    public JsonNode LastNode = new JsonObject();
    public bool LastResult;
    public string[] LastStringArray = [];
}

public class InLongContext
{
    public long LastA;
    public long?[] LastLongArray = [];
    public JsonNode LastNode = new JsonObject();
    public bool LastResult;
}
