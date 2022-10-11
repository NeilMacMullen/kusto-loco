// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.Json.Nodes;
using BabyKusto.Server.Contract;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;

namespace BabyKusto.Server.Service
{
    public class ManagementEndpointHelper
    {
        private readonly IBabyKustoServerState _state;

        public ManagementEndpointHelper(IBabyKustoServerState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public KustoApiResult Process(KustoApiMgmtRequestBody body)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            if (body.Csl == null)
            {
                throw new ArgumentNullException($"{nameof(body)}.{nameof(body.Csl)}");
            }

            var code = KustoCode.ParseAndAnalyze(body.Csl, _state.Globals);

            var diagnostics = code.GetDiagnostics();
            if (diagnostics.Count > 0)
            {
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine($"Kusto diagnostics: {diag.Severity} {diag.Code} {diag.Message} {diag.Description}");
                }

                throw new InvalidOperationException($"Query is malformed.\r\n{string.Join("\r\n", diagnostics.Select(diag => $"{diag.Severity} {diag.Code} {diag.Message} {diag.Description}"))}");
            }

            if (code.Syntax is not CommandBlock commandBlock)
            {
                throw new InvalidOperationException($"Expected a command block, found {code.Syntax.GetType().FullName}.");
            }

            if (commandBlock.Statements.Count != 1)
            {
                throw new InvalidOperationException($"Espected a single statement in command block, found {commandBlock.Statements.Count}.");
            }

            var statement = commandBlock.Statements[0].Element;
            if (statement is not ExpressionStatement expressionStatement)
            {
                throw new InvalidOperationException($"Expected an expression statement, found {statement.GetType().FullName}.");
            }
            return ProcessStatement(body, expressionStatement);
        }

        private KustoApiResult ProcessStatement(KustoApiMgmtRequestBody body, ExpressionStatement statement)
        {
            var expression = statement.Expression;
            if (expression is CommandAndSkippedTokens commandAndSkippedTokens)
            {
                expression = commandAndSkippedTokens.Command;
            }

            if (expression is not CustomCommand customCommand)
            {
                throw new InvalidOperationException($"Expected a custom command expression, found {statement.Expression.GetType().FullName}.");
            }

            var command = _state.Globals.GetCommand(customCommand.CommandKind);
            if (command is null)
            {
                throw new InvalidOperationException($"Unknown command {customCommand.CommandKind}.");
            }

            if (command == EngineCommands.ShowVersion)
            {
                return ProcessShowVersionCommand();
            }
            else if (command == EngineCommands.ShowClusterDatabases)
            {
                return ProcessShowClusterDatabasesCommand();
            }
            else if (command == EngineCommands.ShowSchema)
            {
                return ProcessShowSchemaCommand();
            }
            else if (command == EngineCommands.ShowDatabasesSchemaAsJson)
            {
                return ProcessShowSchemaCommand();
            }

            throw new NotImplementedException($"Command {command.Name} is not yet implemented.");
        }

        private KustoApiResult ProcessShowVersionCommand()
        {
            var result = new KustoApiResult();
            result.Tables.Add(
                new KustoApiTableResult
                {
                    TableName = "Table_0",
                    Columns = {
                        KustoApiColumnDescription.Create("BuildVersion", ScalarTypes.String),
                        KustoApiColumnDescription.Create("BuildTime", ScalarTypes.DateTime),
                        KustoApiColumnDescription.Create("ServiceType", ScalarTypes.String),
                        KustoApiColumnDescription.Create("ProductVersion", ScalarTypes.String),
                        KustoApiColumnDescription.Create("ServiceOffering", ScalarTypes.String),
                    },
                    Rows =
                    {
                        new JsonArray(JsonValue.Create("0.0.0"), JsonValue.Create("2022-10-07T23:00:00Z"), JsonValue.Create("Engine"), JsonValue.Create("0.0.0"), JsonValue.Create("{\"Type\":\"Azure Data Explorer\"}")),
                    },
                });
            return result;
        }

        private KustoApiResult ProcessShowClusterDatabasesCommand()
        {
            var result = new KustoApiResult();
            result.Tables.Add(
            new KustoApiTableResult
            {
                TableName = "Table_0",
                Columns = {
                    KustoApiColumnDescription.Create("DatabaseName", ScalarTypes.String),
                    KustoApiColumnDescription.Create("PersistentStorage", ScalarTypes.String),
                    KustoApiColumnDescription.Create("Version", ScalarTypes.String),
                    KustoApiColumnDescription.Create("IsCurrent", ScalarTypes.Bool),
                    KustoApiColumnDescription.Create("DatabaseAccessMode", ScalarTypes.String),
                    KustoApiColumnDescription.Create("PrettyName", ScalarTypes.String),
                    KustoApiColumnDescription.Create("ReservedSlot1", ScalarTypes.Bool),
                    KustoApiColumnDescription.Create("DatabaseId", ScalarTypes.Guid),
                    KustoApiColumnDescription.Create("InTransitionTo", ScalarTypes.String),
                },
                Rows =
                {
                    new JsonArray(
                        JsonValue.Create(_state.Options.DatabaseName),
                        JsonValue.Create(""),
                        JsonValue.Create("v1.0"),
                        JsonValue.Create(false),
                        JsonValue.Create("ReadWrite"),
                        JsonValue.Create<string>(null),
                        JsonValue.Create<string>(null),
                        JsonValue.Create(_state.Options.DatabaseId),
                        JsonValue.Create("")
                    )
                },
            });
            return result;
        }

        private KustoApiResult ProcessShowSchemaCommand()
        {
            var jsonSchema = JsonSchemaHelper.GetJsonSchema(_state.Options, _state.Tables);

            var result = new KustoApiResult();
            result.Tables.Add(
                new KustoApiTableResult
                {
                    TableName = "Table_0",
                    Columns = {
                        KustoApiColumnDescription.Create("ClusterSchema", ScalarTypes.String),
                    },
                    Rows =
                    {
                        new JsonArray(JsonValue.Create(jsonSchema)),
                    },
                });
            return result;
        }
    }
}
