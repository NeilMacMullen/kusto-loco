using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "make_datetime")]
internal partial class MakeDateTime
{

    internal static DateTime Impl(long year,long month,long day,long hour,long minute,double second)
        => new((int)year,(int)month,(int)day, (int)hour,
            (int)minute,(int)second, DateTimeKind.Utc);

    internal static DateTime AImpl(long year, long month, long day, long hour, long minute)
        => MakeDateTime.Impl(year,month, day, hour, minute, 0.0);
    internal static DateTime BImpl(long year, long month, long day)
        => MakeDateTime.AImpl(year, month, day, 0, 0);

}
