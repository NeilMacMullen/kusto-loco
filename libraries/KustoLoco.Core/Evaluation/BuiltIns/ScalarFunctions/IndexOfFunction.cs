using System;
using System.Diagnostics.Tracing;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.IndexOf")]
internal partial class IndexOfFunction
{

    public static long Do(string source, string substring, long start, long length, long occurence)
    {
        var offset = 0L;
        if (start <0)
            start = source.Length + start;
        if (start > 0)
        {
            if (start >= source.Length)
            {
                return -1;
            }
            offset = start;
            source = source.Substring((int)offset);
        }

        if (length >= 0)
        {
            length =Math.Min(length, source.Length - offset);
            source = source.Substring(0, (int)length);
        }
        while (occurence >= 1)
        {
            var i = source.IndexOf(substring, StringComparison.InvariantCulture);
            if (i < 0)
                return -1;
            if (occurence == 1)
            {
                return i + offset;
            }

           
            if (source.Length <= i + 1)
                return -1;
            source = source.Substring(i +1);
            offset += i+1;
            occurence--;
        }

        return -1;
    }

    public long Impl(string source, string substring) => IndexOfFunction.Do(source, substring, 0,-1,1);
    public long StartImpl(string source, string substring,long start) => IndexOfFunction.Do(source, substring, start,-1,1);
    public long StartLengthImpl(string source, string substring,long start,long length) => IndexOfFunction.Do(source, substring, start,length,1);
    public long StartLengthOccurenceImpl(string source, string substring,long start,long length,long occurence) => IndexOfFunction.Do(source, substring, start,length,occurence);

}
