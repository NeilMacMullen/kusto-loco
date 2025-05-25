namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

public class NumericAggregate
{
    /// <summary>
    /// the number of items
    /// </summary>
    public int Count;
    /// <summary>
    /// used for aggregations such as average
    /// </summary>
    public double DoubleValue;
    /// <summary>
    /// used for aggregations such as max/min for
    /// longs where precision could otherwise be lost
    /// </summary>
    public long LongValue;
    /// <summary>
    /// used for aggregations for decimals
    /// </summary>
    public decimal DecimalValue;
}

