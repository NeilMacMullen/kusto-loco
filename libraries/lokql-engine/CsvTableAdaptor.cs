using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class CsvTableAdaptor(KustoSettingsProvider settings, IProgress<string> progressReporter)
    : TableAdaptorBase(
        CsvSerializer.Default(settings, progressReporter),
        "Csv", "Csv Files", "csv"
    );
