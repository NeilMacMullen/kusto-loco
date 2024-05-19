using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class CsvTableAdaptor(KustoSettingsProvider settings, IKustoConsole console)
    : TableAdaptorBase(
        CsvSerializer.Default(settings, console),
        "Csv", "Csv Files", "csv"
    );
