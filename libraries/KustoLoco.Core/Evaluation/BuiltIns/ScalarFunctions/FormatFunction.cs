using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "format")]
internal partial class FormatFunction
{
    private static string BoolImpl(bool s, string fmt) => s.ToString();
    private static string LongImpl(long s,string fmt) => s.ToString(fmt);
    private static string IntImpl(int s, string fmt) => s.ToString(fmt);
    private static string RealImpl(double s, string fmt) => s.ToString(fmt);
    private static string GuidImpl(Guid s, string fmt) => s.ToString(fmt);
    private static string DateTimeImpl(DateTime s, string fmt) => s.ToString(fmt);
    private static string TimeSpanImpl(TimeSpan s, string fmt) => s.ToString(fmt);
    private static string JsonImpl(JsonNode s, string fmt) => s.ToString();
    private static string DecImpl(decimal s, string fmt) => s.ToString(fmt);
}
