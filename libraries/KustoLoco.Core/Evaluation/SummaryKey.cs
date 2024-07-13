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
    private object? O5;
    private object? O6;
    private object? O7;
    private object? O8;
    private object? O9;

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
    /// <summary>
    /// Sets a value in the key
    /// </summary>
    /// <remarks>
    /// Use with care.  SummaryKey is a struct so
    /// Set only works locally or in preparation for passing
    /// the record to a method.
    /// </remarks>
    public void Set(int i, object? o)
    {
        if (i >= num)
            num = i + 1;
        switch (i)
        {
            case 0:
                O0 = o;
                break;
            case 1:
                O1 = o;
                break;
            case 2:
                O2 = o;
                break;
            case 3:
                O3 = o;
                break;
            case 4:
                O4 = o;
                break;
            case 5:
                O5 = o;
                break;
            case 6:
                O6 = o;
                break;
            case 7:
                O7 = o;
                break;
            case 8:
                O8 = o;
                break;
            case 9:
                O9 = o;
                break;
            case > 9:
                throw new NotImplementedException("summarize operator is limited to 10 properties");
        }
    }
    /// <summary>
    /// Creates an array of the values in the key
    /// </summary>
    public object?[] GetArray()
    {
        var ret = new object?[num];
        if (num > 0) ret[0] = O0;
        if (num > 1) ret[1] = O1;
        if (num > 2) ret[2] = O2;
        if (num > 3) ret[3] = O3;
        if (num > 4) ret[4] = O4;
        if (num > 5) ret[5] = O5;
        if (num > 6) ret[6] = O6;
        if (num > 7) ret[7] = O7;
        if (num > 8) ret[8] = O8;
        if (num > 9) ret[9] = O9;
        return ret;
    }
}
