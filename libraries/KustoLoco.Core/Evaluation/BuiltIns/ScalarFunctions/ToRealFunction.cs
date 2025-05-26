namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToReal")]
internal partial class ToRealFunction
{
    private static double DecImpl(decimal input) => (double)input;
    private static double IntImpl(int input) => input;
    private static double LongImpl(long input) => input;
    private static double DoubleImpl(double input) => input;

    private static double? Impl(string input) =>
        double.TryParse(input, out var parsedResult) ? parsedResult : null;
}
