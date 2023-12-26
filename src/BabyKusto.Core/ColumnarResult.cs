namespace BabyKusto.Core.Evaluation;

public sealed class ColumnarResult : EvaluationResult
{
    public ColumnarResult(BaseColumn column)
        : base(column.Type) =>
        Column = column;

    public BaseColumn Column { get; }
    public bool IsSingleValue => Column.IsSingleValue;

    public override int RowCount => Column.RowCount;

    public ColumnarResult SliceToTopRow() => new(Column.Slice(0, 1));

    public EvaluationResult Inflate(int logicalRowCount) =>
        new ColumnarResult(ColumnFactory.Inflate(Column, logicalRowCount));
}