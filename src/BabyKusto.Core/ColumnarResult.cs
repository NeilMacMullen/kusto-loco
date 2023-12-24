namespace BabyKusto.Core.Evaluation;

public sealed class ColumnarResult : EvaluationResult
{
    public ColumnarResult(BaseColumn column)
        : base(column.Type) =>
        Column = column;

    public BaseColumn Column { get; }
}