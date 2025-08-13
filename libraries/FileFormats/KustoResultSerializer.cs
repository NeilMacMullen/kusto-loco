using System.Collections.Immutable;
using KustoLoco.Core;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;

namespace KustoLoco.FileFormats;

/// <summary>
///     Allows a KustoQueryResult to be serialized for transfer across the wire
/// </summary>
/// <remarks>
///     Although the base class is capable of using any ITableSerializer implementation,
///     it's strongly recommended to use a ParquetSerializer
/// </remarks>
public class KustoResultSerializer(ITableSerializer serializer, string format)
{
    /// <summary>
    ///     Serialize a result to a KustoResultDto
    /// </summary>
    public async Task<KustoResultDto> Serialize(KustoQueryResult result)
    {
        //TODO - could keep this in a stream by using CryptoStream
        var bytes = await GetBytes(result);
        var str = Convert.ToBase64String(bytes);
        return new KustoResultDto
        {
            Query = result.Query,
            StreamFormat = format,
            ResultStream = str,
            VisualizationChartType = result.Visualization.ChartType,
            VisualizationProperties = result.Visualization.Properties.ToDictionary(),
            Duration = result.QueryDuration.TotalMilliseconds,
            Error = result.Error
        };
    }

    /// <summary>
    ///     Deserialize a KustoResultDto to a KustoQueryResult
    /// </summary>
    public async Task<KustoQueryResult> Deserialize(KustoResultDto dto)
    {
        var text = Convert.FromBase64String(dto.ResultStream);
        //TODO could keep this in a stream by using CryptoStream
        var table = await GetTable(text);
        var visualizationProperties = dto.VisualizationProperties.ToImmutableDictionary();
        var vis = new VisualizationState(dto.VisualizationChartType, visualizationProperties);
        var duration = TimeSpan.FromMilliseconds(dto.Duration);
        return new KustoQueryResult(dto.Query, table, vis, duration, dto.Error);
    }

    public async Task<byte[]> GetBytes(KustoQueryResult result)
    {
        using var stream = new MemoryStream();
        await serializer.SaveTable(stream, result);
        return stream.ToArray();
    }

    private async Task<IMaterializedTableSource> GetTable(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var loadResult = await serializer.LoadTable(stream, "result");
        return (loadResult.Table );
    }

    public async Task<KustoQueryResult> FromBytes(byte[] bytes)
    {
        var loaded = await GetTable(bytes);
        return new KustoQueryResult(string.Empty,
            loaded,
            VisualizationState.Empty, TimeSpan.Zero, string.Empty);
    }
}

/// <summary>
///     A serializer for KustoQueryResults that uses Parquet to encode the result stream
/// </summary>
/// <remarks>
///     This is the recommended serializer for KustoQueryResults as it is both efficient and contains sufficient type
///     information
///     to avoid ambiguity when deserializing
/// </remarks>
public class ParquetResultSerializer(ITableSerializer serializer) : KustoResultSerializer(serializer, Format)
{
    public const string Format = "uuparquet";
    public static ParquetResultSerializer Default => new(ParquetSerializer.Default);
}
