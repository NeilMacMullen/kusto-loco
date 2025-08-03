using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Iff")]
public partial class IffFunction
{
    public static bool? BoolImpl(bool predicate, bool? a, bool? b) => predicate ? a : b;
    public static int? IntImpl(bool predicate, int? a, int? b) => predicate ? a : b;
    public static long? LongImpl(bool predicate, long? a, long? b) => predicate ? a : b;
    public static decimal? DecimalImpl(bool predicate, decimal? a, decimal? b) => predicate ? a : b;
    public static double? DoubleImpl(bool predicate, double? a, double? b) => predicate ? a : b;
    public static string? StringImpl(bool predicate, string? a, string? b) => predicate ? a : b;
    public static Guid? GuidImpl(bool predicate, Guid? a, Guid? b) => predicate ? a : b;
    public static JsonNode? JsonNodeImpl(bool predicate, JsonNode? a, JsonNode? b) => predicate ? a : b;
    public static DateTime? DateTimeImpl(bool predicate, DateTime? a, DateTime? b) => predicate ? a : b;
    public static TimeSpan? TimeSpanImpl(bool predicate, TimeSpan? a, TimeSpan? b) => predicate ? a : b;
}
