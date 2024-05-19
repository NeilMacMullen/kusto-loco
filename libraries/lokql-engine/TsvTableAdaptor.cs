using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class TsvTableAdaptor(KustoSettingsProvider settings, IKustoConsole console)
    : TableAdaptorBase(
        CsvSerializer.Tsv(settings, console),
        "Tsv",
        "Tab-separated data",
        "tsv");
