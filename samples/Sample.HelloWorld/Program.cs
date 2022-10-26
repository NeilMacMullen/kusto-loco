// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BabyKusto.Core;
using BabyKusto.Core.Extensions;

string query = @"
let me = 'baby';
datatable(v:int) [ 0, 5, 2, 1, 4, 3 ]
| project countDown = v + 1
| order by countDown desc
| extend greeting=substring(strcat('Hello ', me, '-kusto!'), countDown - 1, 100)
";

var engine = new BabyKustoEngine();
var result = engine.Evaluate(query);
result.Dump(Console.Out);
