using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.MinOf")]
internal partial class MinOfFunction
{
    public long L2Impl(long a, long b) => Math.Min(a, b);
    public long L3Impl(long a, long b, long c) => Math.Min(Math.Min(a, b), c);
    public long L4Impl(long a, long b, long c, long d) => Math.Min(Math.Min(Math.Min(a, b), c), d);


    public double R2Impl(double a, double b) => Math.Min(a, b);
    public double R3Impl(double a, double b, double c) => Math.Min(Math.Min(a, b), c);
    public double R4Impl(double a, double b, double c, double d) => Math.Min(Math.Min(Math.Min(a, b), c), d);


    public decimal D2Impl(decimal a, decimal b) => Math.Min(a, b);
    public decimal D3Impl(decimal a, decimal b, decimal c) => Math.Min(Math.Min(a, b), c);
    public decimal D4Impl(decimal a, decimal b, decimal c, decimal d) => Math.Min(Math.Min(Math.Min(a, b), c), d);


   
    public DateTime DT2Impl(DateTime a, DateTime b) => DateAndTimeSpanComparison.Min(a, b);
    public DateTime DT3Impl(DateTime a, DateTime b, DateTime c) => DateAndTimeSpanComparison.Min(DateAndTimeSpanComparison.Min(a, b), c);

    public DateTime DT4Impl(DateTime a, DateTime b, DateTime c, DateTime d)
        => DateAndTimeSpanComparison.Min(DateAndTimeSpanComparison.Min(DateAndTimeSpanComparison.Min(a, b), c), d);

   

    public TimeSpan TS2Impl(TimeSpan a, TimeSpan b) => DateAndTimeSpanComparison.Min(a, b);
    public TimeSpan TS3Impl(TimeSpan a, TimeSpan b, TimeSpan c) => DateAndTimeSpanComparison.Min(DateAndTimeSpanComparison.Min(a, b), c);

    public TimeSpan TS4Impl(TimeSpan a, TimeSpan b, TimeSpan c, TimeSpan d) =>
        DateAndTimeSpanComparison.Min(
            DateAndTimeSpanComparison.Min(DateAndTimeSpanComparison.Min(a, b), c), d);
}
