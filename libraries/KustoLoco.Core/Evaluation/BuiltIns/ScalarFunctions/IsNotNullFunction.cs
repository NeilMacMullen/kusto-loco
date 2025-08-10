using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.IsNotNull", CustomContext = true)]
public sealed partial class IsNotNullFunction
{
    public static bool BoolImpl(bool? predicate) => predicate != null;
    public static bool IntImpl(int? predicate) => predicate != null;
    public static bool LongImpl(long? predicate) => predicate != null;
    public static bool DecimalImpl(decimal? predicate) => predicate != null;
    public static bool DoubleImpl(double? predicate) => predicate != null;
    public static bool StringImpl(string? predicate) => true; //strings are never null
    public static bool GuidImpl(Guid? predicate) => predicate != null;
    public static bool JsonNodeImpl(JsonNode? predicate) => predicate != null;
    public static bool DateTimeImpl(DateTime? predicate) => predicate != null;
    public static bool TimeSpanImpl(TimeSpan? predicate) => predicate != null;
}
