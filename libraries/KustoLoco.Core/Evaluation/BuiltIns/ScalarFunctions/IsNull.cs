//
// Licensed under the MIT License.

using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using System;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

//custom context because implementation is so cheap
[KustoImplementation(Keyword = "Functions.IsNull",CustomContext= true)]
public sealed partial class IsNullFunction
{
    public static bool BoolImpl(bool? predicate) => predicate == null;
    public static bool IntImpl(int? predicate) => predicate == null;
    public static bool LongImpl(long? predicate) => predicate == null;
    public static bool DecimalImpl(decimal? predicate) => predicate == null;
    public static bool DoubleImpl(double? predicate) => predicate == null;
    public static bool StringImpl(string? predicate) => false; //strings are never null
    public static bool GuidImpl(Guid? predicate) => predicate == null;
    public static bool JsonNodeImpl(JsonNode? predicate) => predicate == null;
    public static bool DateTimeImpl(DateTime? predicate) => predicate == null;
    public static bool TimeSpanImpl(TimeSpan? predicate) => predicate == null;
}

//custom context because implementation is so cheap
