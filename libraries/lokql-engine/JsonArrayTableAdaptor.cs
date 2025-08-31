﻿using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

/// <summary>
///     Reads an array of objects in a json file
/// </summary>
public class JsonArrayTableAdaptor(KustoSettingsProvider settings, IKustoConsole console)
    : TableAdaptorBase(
        new JsonObjectArraySerializer(settings, console),
        "JsonObjectArray", "Array of json objects", "json"
    );

public class JsonLTableAdaptor(KustoSettingsProvider settings, IKustoConsole console)
    : TableAdaptorBase(
        new JsonLSerializer(settings, console),
        "JsonL", "Array of json objects in jsonL format", "jsonl"
    );
