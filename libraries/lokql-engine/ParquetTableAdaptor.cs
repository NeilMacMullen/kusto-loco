using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class ParquetTableAdaptor(KustoSettingsProvider settings, IKustoConsole console)
    : TableAdaptorBase(
        new ParquetSerializer(settings, console),
        "Parquet", "Apache Parquet Files", "parquet"
    );
