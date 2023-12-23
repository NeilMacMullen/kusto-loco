namespace BabyKusto.Core.Evaluation;

public sealed class ColumnarResult : EvaluationResult
{
    public ColumnarResult(Column column)
        : base(column.Type) =>
        Column = column;

    public Column Column { get; }
}