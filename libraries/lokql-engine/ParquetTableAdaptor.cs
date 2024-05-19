using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class ParquetTableAdaptor(KustoSettingsProvider settings, IProgress<string> progressReporter)
    : TableAdaptorBase(
        new ParquetSerializer(settings, progressReporter),
        "Parquet", "Apache Parquet Files", "parquet"
    );
