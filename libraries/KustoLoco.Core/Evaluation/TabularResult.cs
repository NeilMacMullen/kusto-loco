using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation;

public sealed class TabularResult : EvaluationResult
{
    public static readonly TabularResult Empty = new(NullTableSource.Instance
        , VisualizationState.Empty);

    private TabularResult(ITableSource value, VisualizationState visualizationState)
        : base(value.Type)
    {
        Value = value;
        VisualizationState = visualizationState;
    }

    public ITableSource Value { get; }

    public VisualizationState VisualizationState { get; }

    public static TabularResult CreateUnvisualized(ITableSource value) => new(value, VisualizationState.Empty);

    public static EvaluationResult CreateWithVisualisation(ITableSource result, VisualizationState vis) =>
        new TabularResult(result, vis);
}