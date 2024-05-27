namespace KustoLoco.Core;

/// <summary>
/// A Dto that represents a *serialized* QueryResult
/// </summary>
/// <remarks>
/// This dto can be used to efficiently serialize query results.  The ResultStream is a base64 encoded
/// "file" that contains the result data. Typically this will be a Parquet file.  It's possible to use other
/// encodings but Parquet is both efficient and contains sufficient type information to deserialize the result
/// The StreamFormat is a string that describes the format of the ResultStream
///
/// Visualisation state is transferred so that the client can choose to render the result in the format intended
/// in the query 
/// </remarks>
public class KustoResultDto
{
    public double Duration { get; set; }
    public string Query { get; set; } = string.Empty;
    public string StreamFormat { get; set; } = string.Empty;
    public string ResultStream { get; set; } = string.Empty;
    public string VisualizationChartType { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
