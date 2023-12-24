using BabyKusto.Core.Evaluation;

namespace KustoSupport
{
    /// <summary>
    ///     Provides the results of a Kusto query as a collection of dictionaries
    /// </summary>
    /// <remarks>
    ///     The client really needs to have some idea of the expected schema to use this layer of API
    ///     In the future we may provide a more "column-oriented" result where the type of the columns
    ///     is explicitly provided but for the moment this allows easy json serialisation
    /// </remarks>
    public readonly record struct KustoQueryResult<T>
    {
        /// <summary>
        ///     Provides the results of a Kusto query as a collection of dictionaries
        /// </summary>
        /// <remarks>
        ///     The client really needs to have some idea of the expected schema to use this layer of API
        ///     In the future we may provide a more "column-oriented" result where the type of the columns
        ///     is explicitly provided but for the moment this allows easy json serialisation
        /// </remarks>
        public KustoQueryResult(string Query,
            IReadOnlyCollection<T> Results,
            int QueryDuration,
            string Error)
        {
            this.Query = Query;
            this.Results = Results;
            this.QueryDuration = QueryDuration;
            this.Error = Error;
        }

        public KustoQueryResult(string Query,
            IReadOnlyCollection<T> Results,
            VisualizationState visualization,
            int QueryDuration,
            string Error)
        {
            this.Query = Query;
            this.Results = Results;
            this.QueryDuration = QueryDuration;
            this.Error = Error;
            Visualization = visualization;
        }

        public string Query { get; init; }
        public IReadOnlyCollection<T> Results { get; init; }
        public int QueryDuration { get; init; }
        public string Error { get; init; }
        public VisualizationState Visualization { get; init; } = VisualizationState.Empty;
    }
}