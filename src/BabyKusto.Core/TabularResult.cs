namespace BabyKusto.Core.Evaluation;

public sealed class TabularResult : EvaluationResult
{
    public static readonly TabularResult Empty = new(NullTableSource.Instance
        , null);

    public TabularResult(ITableSource value, VisualizationState? visualizationState)
        : base(value.Type)
    {
        Value = value;
        VisualizationState = visualizationState;
    }

    public ITableSource Value { get; }

    public VisualizationState? VisualizationState { get; }
}