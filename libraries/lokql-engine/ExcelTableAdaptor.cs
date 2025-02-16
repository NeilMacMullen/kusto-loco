using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class ExcelTableAdaptor(KustoSettingsProvider settings, IKustoConsole console)
    : TableAdaptorBase(
        new ExcelSerializer(settings, console),
        "Excel",
        "Excel data",
        "xls xlsx");
