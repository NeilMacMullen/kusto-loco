using System;

namespace KustoLoco.Core.Evaluation;

public record struct SummaryKey
{
    private int num;
    private object? O0;
    private object? O1;
    private object? O2;
    private object? O3;
    private object? O4;

    public SummaryKey()
    {
    }

    public SummaryKey(object?[] objects)
    {
        for (var i = 0; i < objects.Length; i++)
        {
            Set(i, objects[i]);
        }
    }

    public void Set(int i, object? o)
    {
        if (i >= num)
            num = i + 1;
        if (i == 0) O0 = o;
        if (i == 1) O1 = o;
        if (i == 2) O2 = o;
        if (i == 3) O3 = o;
        if (i == 4) O4 = o;
        if (i > 4)
            throw new NotImplementedException("summarize limited to 5 vals");
    }

    public object?[] GetArray()
    {
        var ret = new object?[num];
        if (num > 0) ret[0] = O0;
        if (num > 1) ret[1] = O1;
        if (num > 2) ret[2] = O2;
        if (num > 3) ret[3] = O3;
        if (num > 4) ret[4] = O4;
        return ret;
    }
}