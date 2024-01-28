// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.Avg")]
internal partial class MyAvgAggregate
{
    internal static void IntImpl(AvgAcc context, int n)
    {
        context.Count++;
        context.Total += n;
    }

    internal static double? IntImplFinish(AvgAcc context) => context.Count == 0
        ? null
        : context.Total / context.Count;
}