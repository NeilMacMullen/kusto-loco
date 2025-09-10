using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

public class EmptyOrderablePartitioner : OrderablePartitioner<Tuple<int, int>>
{
    public EmptyOrderablePartitioner()
        : base(true, true, true) { }

    public override IList<IEnumerator<KeyValuePair<long, Tuple<int, int>>>> GetOrderablePartitions(int partitionCount)
    {
        var list = new List<IEnumerator<KeyValuePair<long, Tuple<int, int>>>>();
        for (int i = 0; i < partitionCount; i++)
        {
            list.Add(EmptyEnumerator());
        }
        return list;
    }

    public override IEnumerable<KeyValuePair<long, Tuple<int, int>>> GetOrderableDynamicPartitions()
    {
        return new List<KeyValuePair<long, Tuple<int, int>>>();
    }

    public override bool SupportsDynamicPartitions => true;

    private IEnumerator<KeyValuePair<long, Tuple<int, int>>> EmptyEnumerator()
    {
        yield break;
    }
}
