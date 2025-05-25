using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.MaxOf")]
internal partial class MaxOfFunction
{
    public long L2Impl(long a, long b) => Math.Max(a, b);
    public long L3Impl(long a, long b, long c) => Math.Max(Math.Max(a, b), c);
    public long L4Impl(long a, long b, long c, long d) => Math.Max(Math.Max(Math.Max(a, b), c), d);


    public double R2Impl(double a, double b) => Math.Max(a, b);
    public double R3Impl(double a, double b, double c) => Math.Max(Math.Max(a, b), c);
    public double R4Impl(double a, double b, double c, double d) => Math.Max(Math.Max(Math.Max(a, b), c), d);


    public decimal D2Impl(decimal a, decimal b) => Math.Max(a, b);
    public decimal D3Impl(decimal a, decimal b, decimal c) => Math.Max(Math.Max(a, b), c);
    public decimal D4Impl(decimal a, decimal b, decimal c, decimal d) => Math.Max(Math.Max(Math.Max(a, b), c), d);

    

    public DateTime DT2Impl(DateTime a, DateTime b) => DateAndTimeSpanComparison.Max(a, b);
    public DateTime DT3Impl(DateTime a, DateTime b, DateTime c) => DateAndTimeSpanComparison.Max(DateAndTimeSpanComparison.Max(a, b), c);

    public DateTime DT4Impl(DateTime a, DateTime b, DateTime c, DateTime d)
        => DateAndTimeSpanComparison.Max(DateAndTimeSpanComparison.Max(DateAndTimeSpanComparison.Max(a, b), c), d);

  

    public TimeSpan TS2Impl(TimeSpan a, TimeSpan b) => DateAndTimeSpanComparison.Max(a, b);
    public TimeSpan TS3Impl(TimeSpan a, TimeSpan b, TimeSpan c) => DateAndTimeSpanComparison.Max(DateAndTimeSpanComparison.Max(a, b), c);

    public TimeSpan TS4Impl(TimeSpan a, TimeSpan b, TimeSpan c, TimeSpan d) =>
        DateAndTimeSpanComparison.Max(
            DateAndTimeSpanComparison.Max(DateAndTimeSpanComparison.Max(a, b), c), d);
}
