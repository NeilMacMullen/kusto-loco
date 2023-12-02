using System;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
    private readonly record struct SummaryKey
    {
        private readonly object? O0;
        private readonly object? O1;
        private readonly object? O2;
        private readonly object? O3;
        private readonly object? O4;

        public SummaryKey(object?[] Values)
        {
            if (Values.Length > 0)
                O0 = Values[0];
            if (Values.Length > 1)
                O1 = Values[1];
            if (Values.Length > 2)
                O2 = Values[2];
            if (Values.Length > 3)
                O3 = Values[3];
            if (Values.Length > 4)
                O4 = Values[4];
            if (Values.Length > 5)
                throw new NotImplementedException("summarize limited to 5 vals");
        }
    }
}