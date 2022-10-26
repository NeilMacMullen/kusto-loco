// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BabyKusto.Core.Evaluation;
using BabyKusto.Server.Contract;
using Kusto.Language.Symbols;
using Microsoft.AspNetCore.Http;

namespace BabyKusto.Server.Service
{
    public class QueryV2EndpointHelper
    {
        private readonly IBabyKustoServerState _state;

        public QueryV2EndpointHelper(IBabyKustoServerState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

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
                throw new InvalidOperationException($"Expected tabular result, found {(queryResult is null ? "null" : queryResult.GetType().FullName)}.");
            }

            try
            {
                var jsonWriter = new Utf8JsonWriter(context.Response.Body);
                {
                    using var writer = new KustoQueryV2ResponseWriter(jsonWriter);
                    {
                        await writer.StartAsync();
                        await WritePrimaryResultTable(tabularResult, writer);
                        await writer.FinishAsync();
                    }
                    await jsonWriter.DisposeAsync();
                }
            }
            catch (Exception)
            {
                context.Abort();
                throw;
            }
        }

        private static async Task WritePrimaryResultTable(TabularResult tabularResult, KustoQueryV2ResponseWriter writer)
        {
            var resultType = (TableSymbol)tabularResult.Type;
            using (var tableWriter = writer.CreateTableWriter())
            {
                await tableWriter.StartAsync(0, KustoQueryV2ResponseTableKind.PrimaryResult, "PrimaryResult", resultType.Columns.Select(c => new KustoApiV2ColumnDescription { ColumnName = c.Name, ColumnType = c.Type.Display }).ToList());
                foreach (var chunk in tabularResult.Value.GetData())
                {
                    for (int i = 0; i < chunk.RowCount; i++)
                    {
                        tableWriter.StartRow();
                        for (int c = 0; c < chunk.Columns.Length; c++)
                        {
                            tableWriter.WriteRowValue(JsonValue.Create(chunk.Columns[c].RawData.GetValue(i)));
                        }
                        tableWriter.EndRow();
                        await tableWriter.FlushAsync();
                    }
                }

                await tableWriter.FinishAsync();
            }
        }
    }
}
