using System;
using System.Threading.Tasks;

// ReSharper disable PartialTypeWithSinglePart


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Sin")]
internal partial class SinFunction
{
    private static double Impl(SinContext context,double input)
    {
        if (context.Previous == input)
            return context.PreviousResult;

        var result= Math.Sin(input);
        context.Previous= input;
        context.PreviousResult = result;
        return result;
    }
}

public class SinContext
{
    public double Previous ;
    public double PreviousResult;

    public SinContext()
    {
        Previous = 0;
        PreviousResult = 0;
    }
}
