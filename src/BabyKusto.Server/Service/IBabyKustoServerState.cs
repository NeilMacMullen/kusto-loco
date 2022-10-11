// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using BabyKusto.Core;
using Kusto.Language;

namespace BabyKusto.Server.Service
{
    public interface IBabyKustoServerState
    {
        GlobalState Globals { get; }
        BabyKustoServerOptions Options { get; }
        List<ITableSource> Tables { get; }

        BabyKustoEngine Engine { get; }
    }
}