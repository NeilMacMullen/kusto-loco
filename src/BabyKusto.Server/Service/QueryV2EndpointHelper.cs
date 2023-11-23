// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BabyKusto.Core.Evaluation;
using BabyKusto.Server.Contract;
using Kusto.Language.Symbols;
using Microsoft.AspNetCore.Http;

namespace BabyKusto.Server.Service;

public class QueryV2EndpointHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly IBabyKustoServerState _state;

    public QueryV2EndpointHelper(IBabyKustoServerState state) =>
        _state = state ?? throw new ArgumentNullException(nameof(state));

    public async Task Process(KustoApiQueryRequestBody body, HttpContext context)
    {
        _ = body ?? throw new ArgumentNullException(nameof(body));
        if (body.Csl == null)
        {
            throw new ArgumentNullException($"{nameof(body)}.{nameof(body.Csl)}");
        }

        var queryResult = _state.Engine.Evaluate(body.Csl);
        if (queryResult is not TabularResult tabularResult)
        {
            throw new InvalidOperationException(
                $"Expected tabular result, found {(queryResult is null ? "null" : queryResult.GetType().FullName)}.");
        }

        try
        {
            var jsonWriter = new Utf8JsonWriter(context.Response.Body);

            var writer = new KustoQueryV2ResponseWriter(jsonWriter);
            await writer.StartAsync();
            await WritePrimaryResultTable(tabularResult, writer);
            await WriteQueryPropertiesTable(tabularResult, writer);
            await writer.FinishAsync();

            await jsonWriter.DisposeAsync();
        }
        catch
        {
            context.Abort();
            throw;
        }
    }

    private static async Task WritePrimaryResultTable(TabularResult tabularResult,
        KustoQueryV2ResponseWriter writer)
    {
        var resultType = (TableSymbol)tabularResult.Type;
        var tableWriter = writer.CreateTableWriter();
        await tableWriter.StartAsync(0, KustoQueryV2ResponseTableKind.PrimaryResult, "PrimaryResult",
            resultType.Columns.Select(c => new KustoApiV2ColumnDescription
                { ColumnName = c.Name, ColumnType = SchemaDisplay.GetText(c.Type) }).ToList());
        foreach (var chunk in tabularResult.Value.GetData())
        {
            for (var i = 0; i < chunk.RowCount; i++)
            {
                tableWriter.StartRow();
                for (var c = 0; c < chunk.Columns.Length; c++)
                {
                    var v = chunk.Columns[c].RawData.GetValue(i);

                    JsonValue? valueToWrite;
                    if (v is double doubleV)
                    {
                        if (double.IsPositiveInfinity(doubleV))
                        {
                            valueToWrite = JsonValue.Create("Infinity");
                        }
                        else if (double.IsNegativeInfinity(doubleV))
                        {
                            valueToWrite = JsonValue.Create("-Infinity");
                        }
                        else if (double.IsNaN(doubleV))
                        {
                            valueToWrite = JsonValue.Create("NaN");
                        }
                        else
                        {
                            valueToWrite = JsonValue.Create(v);
                        }
                    }
                    else if (v is JsonNode json)
                    {
                        valueToWrite = JsonValue.Create(json.ToJsonString(JsonOptions));
                    }
                    else
                    {
                        valueToWrite = JsonValue.Create(v);
                    }

                    tableWriter.WriteRowValue(valueToWrite);
                }

                tableWriter.EndRow();
                await tableWriter.FlushAsync();
            }
        }

        await tableWriter.FinishAsync();
    }

    private static async Task WriteQueryPropertiesTable(TabularResult tabularResult,
        KustoQueryV2ResponseWriter writer)
    {
        if (tabularResult.VisualizationState != null)
        {
            var tableType = new TableSymbol(
                "QueryProperties",
                new ColumnSymbol("TableIndex", ScalarTypes.Long),
                new ColumnSymbol("Type", ScalarTypes.String),
                new ColumnSymbol("Value", ScalarTypes.String)
            );
            var resultType = tableType;
            var tableWriter = writer.CreateTableWriter();
            await tableWriter.StartAsync(1, KustoQueryV2ResponseTableKind.QueryProperties, "QueryProperties",
                tableType.Columns.Select(c => new KustoApiV2ColumnDescription
                    { ColumnName = c.Name, ColumnType = SchemaDisplay.GetText(c.Type) }).ToList());

            tableWriter.StartRow();
            tableWriter.WriteRowValue(JsonValue.Create(0));
            tableWriter.WriteRowValue(JsonValue.Create("Visualization"));
            tableWriter.WriteRowValue(JsonValue.Create(JsonSerializer.Serialize(
                new ChartVisualizationDto
                {
                    Visualization = tabularResult.VisualizationState.ChartType,
                    Kind = tabularResult.VisualizationState.ChartKind,
                    // TODO: We should set a lot more things here, but this seems to work well enough for now and Kusto Explorer fills in the gaps...
                })));
            tableWriter.EndRow();

            await tableWriter.FlushAsync();
            await tableWriter.FinishAsync();
        }
    }

    private class ChartVisualizationDto
    {
        public string? Visualization { get; set; }

        public string? Kind { get; set; }

        public string? YSplit { get; set; }

        public string? Legend { get; set; }

        public string? XAxis { get; set; }

        public string? YAxis { get; set; }

        public string? Title { get; set; }

        public string? XColumn { get; set; }

        public string[]? Series { get; set; }

        public string[]? YColumns { get; set; }

        public string[]? AnomalyColumns { get; set; }

        public string? XTitle { get; set; }

        public string? YTitle { get; set; }

        public bool Accumulate { get; set; }

        public bool IsQuerySorted { get; set; }

        public object? Ymin { get; set; } = "NaN";
        public object? Ymax { get; set; } = "NaN";
        public object? Xmin { get; set; }
        public object? Xmax { get; set; }
    }
}