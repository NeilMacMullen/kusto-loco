using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class TsvTableAdaptor(KustoSettingsProvider settings, IProgress<string> progressReporter)
    : TableAdaptorBase(
        CsvSerializer.Tsv(settings, progressReporter),
        "Tsv",
        "Tab-separated data",
        "tsv");
