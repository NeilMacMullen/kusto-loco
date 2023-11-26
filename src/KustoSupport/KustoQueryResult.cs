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
    public readonly record struct KustoQueryResult<T>(string Query,
        IReadOnlyCollection<T> Results,
        int QueryDuration,
        string Error);
}
