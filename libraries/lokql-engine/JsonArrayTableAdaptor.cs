using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using NLog;
using NotNullStrings;

namespace Lokql.Engine;

/// <summary>
///     Reads an array of objects in a json file
/// </summary>
public class JsonArrayTableAdaptor(KustoSettingsProvider settings, IProgress<string> progressReporter)
    : TableAdaptorBase(
        new JsonObjectArraySerializer(settings, progressReporter),
        "JsonObjectArray", "Array of json objects", "json"
    );
