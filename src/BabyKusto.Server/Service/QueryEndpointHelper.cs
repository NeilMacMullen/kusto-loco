// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Nodes;
using BabyKusto.Core.Evaluation;
using BabyKusto.Server.Contract;
using Kusto.Language.Symbols;

namespace BabyKusto.Server.Service
{
    public class QueryEndpointHelper
    {
        private readonly IBabyKustoServerState _state;

        public QueryEndpointHelper(IBabyKustoServerState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public KustoApiResult Process(KustoApiQueryRequestBody body)
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

            var resultType = (TableSymbol)tabularResult.Type;

            var resultTable = new KustoApiTableResult
            {
                TableName = "Table_0",
            };
            foreach (var col in resultType.Columns)
            {
                resultTable.Columns.Add(KustoApiColumnDescription.Create(col.Name, col.Type));
            }

            foreach (var chunk in tabularResult.Value.GetData())
            {
                for (int i = 0; i < chunk.RowCount; i++)
                {
                    var rowValues = new JsonArray();
                    for (int c = 0; c < chunk.Columns.Length; c++)
                    {
                        rowValues.Add(JsonValue.Create(chunk.Columns[c].RawData.GetValue(i)));
                    }

                    resultTable.Rows.Add(rowValues);
                }
            }

            return new KustoApiResult
            {
                Tables = { resultTable },
            };
        }
    }
}
