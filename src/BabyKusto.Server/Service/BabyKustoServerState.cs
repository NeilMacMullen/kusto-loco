// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using BabyKusto.Core;
using Kusto.Language;
using Kusto.Language.Symbols;
using Microsoft.Extensions.Options;

namespace BabyKusto.Server.Service
{
    internal class BabyKustoServerState : IBabyKustoServerState
    {
        public BabyKustoServerState(IOptions<BabyKustoServerOptions> options, ITablesProvider tablesProvider)
        {
            _ = tablesProvider ?? throw new ArgumentNullException(nameof(tablesProvider));

            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            Tables = tablesProvider.GetTables();
            Globals = GlobalState.Default.WithDatabase(
                new DatabaseSymbol(Options.DatabaseName, Tables.Select(t => t.Type).ToArray()));
            
            var engine = new BabyKustoEngine();
            foreach (var table in Tables)
            {
                engine.AddGlobalTable(table.Type.Name, table);
            }
            Engine = engine;
        }

        public BabyKustoServerOptions Options { get; }
        public List<ITableSource> Tables { get; }
        public GlobalState Globals { get; }
        public BabyKustoEngine Engine { get; }
    }
}
